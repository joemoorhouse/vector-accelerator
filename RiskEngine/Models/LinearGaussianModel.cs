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
    public class LinearGaussianModel : LinearGaussianModelBase, IDiscountFactor
    {
        public override void Initialise(SimulationGraph graph)
        {
            base.Initialise(graph);
        }
        
        public NArray this[int timeIndex, DateTime t]
        {
            get { return DiscountFactor(timeIndex, t); }
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

            
            double drift = GetDrift(timeIndex, tenor);
            return NMath.Exp(lnDF0t2 - lnDF0t1 - 0.5 * drift + B);
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

            double drift = GetDrift(timeIndex, tenor1) - GetDrift(timeIndex, tenor2);
            var dfRatio = NMath.Exp(lnDF0t1 - lnDF0t2 - 0.5 * drift + B); // df(t, t1) / df(t, t2)
            double alpha = IntervalInYears(t1, t2);
            return (dfRatio - 1) / alpha;
        }

        protected double GetDrift(int timeIndex, double tenor)
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
    }
}
