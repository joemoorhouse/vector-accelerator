using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskEngine.Framework;
using VectorAccelerator;
using RiskEngine.Factors;
using RiskEngine.Data;

namespace RiskEngine.Models
{
    [ModelAppliesTo(typeof(DiscountFactorNonCash))]
    public class LinearGaussianModel : Model, IDiscountFactor
    {
        #region Model Properties

        public Curve ZeroRatesT0 { get; set; }

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

        public override void Initialise(SimulationGraph graph)
        {
            base.Initialise(graph);

            _factorPaths = Enumerable.Range(0, factorCount).Select(i =>
                graph.RegisterModel<MeanRevertingNormalPathModel>(
                string.Format("IR_DiscountFactor_{0}_Factor{1}", Identifier, i)))
                .ToArray();

            _factors = _factorPaths.Select(p => p.Process).ToArray();

            var weightsProvider = graph.Context.Data.CalibrationParametersProviderOfType<WeightsProvider>();

            _factorCorrelation = new double[_factors.Length, _factors.Length];

            for (int i = 0; i < _factorPaths.Length; ++i)
            {
                for (int j = 0; j < _factorPaths.Length; ++j)
                {
                    _factorCorrelation[i, j] = NMath.Dot(
                        weightsProvider.Value(_factorPaths[i].Identifier),
                        weightsProvider.Value(_factorPaths[j].Identifier));
                }
            }
        }

        public override void Prepare(Context context)
        {
            //var simulationT0 = _timePoints.First().DateTime;
            
            //_zeroRateT0 = new Curve(
            //    DiscountFactorT0.Data
            //    .Select(c => new DataPoint(c.Time, -NMath.Log(c.Value) / IntervalInYears(simulationT0, c.Time))
            //    ));

            //_zeroRateT0.Data[0] = new DataPoint(_zeroRateT0[0].Time, _zeroRateT0[1].Value);
        }

        /// <summary>
        /// Calculates discount factor for payment at time t2 at the supplied simulation time index 
        /// </summary>
        /// <param name="timeIndex"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public NArray DiscountFactor(int timeIndex, DateTime t2)
        {
            var B = NArray.CreateScalar(0);
            var simulationT0 = _timePoints.First().DateTime;

            double tenor = IntervalInYears(_timePoints[timeIndex].DateTime, t2);
            
            for (int factorIndex = 0; factorIndex < _factors.Length; ++factorIndex)
            {
                B += Sigma(factorIndex) * E(Lambda(factorIndex), tenor)
                    * _factorPaths[factorIndex][timeIndex];
            }

            var lnDF0t2 = -ZeroRatesT0.GetValue(t2) * IntervalInYears(simulationT0, t2);
            var lnDF0t1 = -ZeroRatesT0.GetValue(_timePoints[timeIndex].DateTime) 
                * _timePoints[timeIndex].YearsFromBaseDate;

            //var d0t2 = DiscountFactorT0.GetValue(t2);
            //var d0t1 = DiscountFactorT0.GetValue(_timePoints[timeIndex].DateTime);
            
            double drift = GetDrift(timeIndex, tenor);
            return NMath.Exp(lnDF0t2 - lnDF0t1 - 0.5 * drift + B);
            //return (d0t2 / d0t1) * NMath.Exp(-0.5 * drift + B);
        }

        /// <summary>
        /// Calculate forward rate that applies between times t1 and t2 (t2 > t1)
        /// </summary>
        /// <param name="timeIndex"></param>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public NArray ForwardRate(int timeIndex, DateTime t1, DateTime t2)
        {
            var B = NArray.CreateScalar(0);
            var simulationT0 = _timePoints.First().DateTime;

            double tenor1 = IntervalInYears(_timePoints[timeIndex].DateTime, t1);
            double tenor2 = IntervalInYears(_timePoints[timeIndex].DateTime, t2);

            for (int factorIndex = 0; factorIndex < _factors.Length; ++factorIndex)
            {
                B += Sigma(factorIndex) * (E(Lambda(factorIndex), tenor1) - E(Lambda(factorIndex), tenor2))
                    * _factorPaths[factorIndex][timeIndex];
            }

            var lnDF0t1 = -ZeroRatesT0.GetValue(t1) * IntervalInYears(simulationT0, t1);
            var lnDF0t2 = -ZeroRatesT0.GetValue(t2) * IntervalInYears(simulationT0, t2);

            //var d0t1 = DiscountFactorT0.GetValue(t1);
            //var d0t2 = DiscountFactorT0.GetValue(t2);

            double drift = GetDrift(timeIndex, tenor1) - GetDrift(timeIndex, tenor2);
            //var dfRatio = (d0t1 / d0t2) * NMath.Exp(-0.5 * drift + B); // df(t, t1) / df(t, t2)
            var dfRatio = NMath.Exp(lnDF0t1 - lnDF0t2 - 0.5 * drift + B); // df(t, t1) / df(t, t2)
            double alpha = IntervalInYears(t1, t2);
            return (dfRatio - 1) / alpha;
        }

        private double GetDrift(int timeIndex, double tenor)
        {
            double drift = 0;
            double t = _timePoints[timeIndex].YearsFromBaseDate;
            for (int i = 0; i < _factors.Length; ++i)
            {
                for (int j = 0; j < _factors.Length; ++j)
                {
                    drift += (_factorCorrelation[i, j] * Sigma(i) * Sigma(j) / (Lambda(i) * Lambda(j)))
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

        private double IntervalInYears(DateTime t1, DateTime t2)
        {
            return (t2 - t1).TotalDays / 365.25;
        }

        MeanRevertingNormalPathModel[] _factorPaths;
        MeanRevertingNormalProcess[] _factors;
        double[,] _factorCorrelation;
        int factorCount;
        //Curve _zeroRateT0;
    }
}
