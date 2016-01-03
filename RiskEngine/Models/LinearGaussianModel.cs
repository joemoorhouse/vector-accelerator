using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskEngine.Framework;
using VectorAccelerator;

namespace RiskEngine.Models
{
    public class LinearGaussianModel : Model
    {
        #region Model Properties

        public double[] Sigma { get; set; }
        public double[] Lambda { get; set; }
        public Curve DiscountFactorT0 { get; set; }

        #endregion

        /// <summary>
        /// Calculates discount factor for time t, at the current simulation time
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public NArray DiscountFactor(DateTime t)
        {   
            var B = NArrayFactory.CreateLike(_factors[0].Value);
            double tenor = (t - CurrentTimePoint).Days / 365.25;
            for (int factorIndex = 0; factorIndex < _factors.Length; ++factorIndex)
            {
                B += Sigma[factorIndex] * E(Lambda[factorIndex], tenor)
                    * _factors[factorIndex].Value;
            }
            var d0T = DiscountFactorT0.GetValue(t);
            var d0t = DiscountFactorT0.GetValue(CurrentTimePoint);
            double drift = GetDrift();
            return (d0T / d0t) * NMath.Exp(-0.5 * drift + B);
        }

        private double GetDrift()
        {
            double drift = 0;
            double t = 0, T = 0; // = CurrentTimePoint - 
            for (int i = 0; i < _factors.Length; ++i)
            {
                for (int j = 0; j < _factors.Length; ++j)
                {
                    drift = (_factorCorrelation[i, j] * Sigma[i] * Sigma[j] / (Lambda[i] * Lambda[j]))
                        * (DriftHelper(Lambda[i], t, T) + DriftHelper(Lambda[j], t, T)
                        - DriftHelper(Lambda[i] + Lambda[j], t, T));
                }
            }
            return drift;
        }

        private static double DriftHelper(double lambda, double t, double T)
        {
            return (1.0 / lambda) * (1 - Math.Exp(-lambda * t)) * (1 - Math.Exp(-lambda * T));
        }

        private static double E(double lambda, double t)
        {
            return (1.0 - Math.Exp(-lambda * t)) / lambda;
        }

        MeanRevertingNormalProcess[] _factors;
        double[,] _factorCorrelation; 
    }
}
