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
    [ModelAppliesTo(typeof(Numeraire))]
    public class NumeraireModel : LinearGaussianModelBase, IEvolvingModel
    {
        public NArray this[int timeIndex] 
        { 
            get 
            { 
                var t = _timePoints[timeIndex].YearsFromBaseDate;
                return NMath.Exp(-ZeroRatesT0.GetValue(_timePoints[timeIndex].DateTime) * t)
                    * _state[timeIndex]; }
        }

        public override void Initialise(SimulationGraph graph)
        {
            base.Initialise(graph);

            _state = new NArray[graph.Context.Settings.SimulationTimePoints.Length];

            _weinerPaths = GetFactorIdentifiers().Select(id =>
                graph.RegisterModel<WienerPathModel>(id))
                .ToArray();
        }

        public void StepNext(TimeInterval timeStep)
        {
            int timeIndex = timeStep.Index;

            var S = NArray.CreateScalar(0);

            double tenor = timeStep.IntervalInYears;

            for (int factorIndex = 0; factorIndex < _factors.Length; ++factorIndex)
            {
                S += (Sigma(factorIndex) / Lambda(factorIndex))
                    * (_factorPaths[factorIndex][timeIndex] + _weinerPaths[factorIndex][timeIndex]);
            }

            double drift = GetDrift(timeIndex);

            _state[timeIndex] = NMath.Exp(-0.5 * drift + S);
        }

        protected double GetDrift(int timeIndex)
        {
            double drift = 0;
            double t = _timePoints[timeIndex].YearsFromBaseDate;
            for (int i = 0; i < _factors.Length; ++i)
            {
                for (int j = 0; j < _factors.Length; ++j)
                {
                    drift += (_factorCorrelation[i, j] * Sigma(i) * Sigma(j) / (Lambda(i) * Lambda(j)))
                        * (t - E(Lambda(i), t) - E(Lambda(j), t) + E(Lambda(i) + Lambda(j), t));
                }
            }
            return drift;
        }

        protected WienerPathModel[] _weinerPaths;
        protected NArray[] _state;
    }
}
