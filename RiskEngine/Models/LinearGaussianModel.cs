using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskEngine.Framework;
using VectorAccelerator;
using RiskEngine.Factors;

namespace RiskEngine.Models
{
    [ModelAppliesTo(typeof(DiscountFactorNonCash))]
    public class LinearGaussianModel : Model, IDiscountFactor
    {
        #region Model Properties

        public Curve DiscountFactorT0 { get; set; }
        public MeanRevertingNormalProcess[] Factors
        {
            get { return _factors; }
        }

        #endregion

        public NArray this[int timeIndex, DateTime t]
        {
            get { return DiscountFactor(timeIndex, t); }
        }

        public LinearGaussianModel()
        {
            factorCount = 3;
        }

        public override void Initialise(Simulation simulation)
        {
            _factorPaths = Enumerable.Range(0, factorCount).Select(i =>
                simulation.RegisterModel<MeanRevertingNormalPathModel>(string.Format("IR.{0}.{1}", Identifier, i)))
                .ToArray();

            _factors = _factorPaths.Select(p => p.Process).ToArray();
        }

        /// <summary>
        /// Calculates discount factor for payment at time t at the supplied simulation time index 
        /// </summary>
        /// <param name="timeIndex"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public NArray DiscountFactor(int timeIndex, DateTime t)
        {
            var B = NArrayFactory.CreateLike(_factorPaths[0][timeIndex]);
            double tenor = (t - _timePoints[timeIndex].DateTime).Days / 365.25;
            
            for (int factorIndex = 0; factorIndex < _factors.Length; ++factorIndex)
            {
                B += Sigma(factorIndex) * E(Lambda(factorIndex), tenor)
                    * _factorPaths[factorIndex][timeIndex];
            }

            var d0T = DiscountFactorT0.GetValue(t);
            var d0t = DiscountFactorT0.GetValue(_timePoints[timeIndex].DateTime);
            
            double drift = GetDrift(timeIndex, tenor);
            return (d0T / d0t) * NMath.Exp(-0.5 * drift + B);
        }

        private double GetDrift(int timeIndex, double tenor)
        {
            double drift = 0;
            double t = _timePoints[timeIndex].YearsFromBaseDate;
            for (int i = 0; i < _factors.Length; ++i)
            {
                for (int j = 0; j < _factors.Length; ++j)
                {
                    drift = (_factorCorrelation[i, j] * Sigma(i) * Sigma(j) / (Lambda(i) * Lambda(j)))
                        * (DriftHelper(Lambda(i), t, tenor) + DriftHelper(Lambda(j), t, tenor)
                        - DriftHelper(Lambda(i) + Lambda(j), t, tenor));
                }
            }
            return drift;
        }

        private double Sigma(int factorIndex)
        {
            return _factors[factorIndex].Sigma;
        }

        private double Lambda(int factorIndex)
        {
            return _factors[factorIndex].Lambda;
        }

        private static double DriftHelper(double lambda, double t, double T)
        {
            return (1.0 / lambda) * (1 - Math.Exp(-lambda * t)) * (1 - Math.Exp(-lambda * T));
        }

        private static double E(double lambda, double t)
        {
            return (1.0 - Math.Exp(-lambda * t)) / lambda;
        }

        MeanRevertingNormalPathModel[] _factorPaths;
        MeanRevertingNormalProcess[] _factors;
        double[,] _factorCorrelation;
        int factorCount;
    }
}
