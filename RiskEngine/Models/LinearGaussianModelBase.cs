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
    /// <summary>
    /// Base class for models based on a Linear Gaussian Model, e.g. discount factor and numeraire. 
    /// </summary>
    public class LinearGaussianModelBase : Model
    {
        #region Model Properties

        public Curve ZeroRatesT0 { get; set; }

        public MeanRevertingNormalProcess[] Factors
        {
            get { return _factors; }
        }

        #endregion

        public LinearGaussianModelBase()
        {
            factorCount = 3;
        }

        public override void Initialise(SimulationGraph graph)
        {
            base.Initialise(graph);

            var factorIdentifiers = GetFactorIdentifiers().ToArray();

            _factorPaths = GetFactorIdentifiers().Select(id =>
                graph.RegisterModel<MeanRevertingNormalPathModel>(id))
                .ToArray();

            _factors = _factorPaths.Select(p => p.Process).ToArray();

            var weightsProvider = graph.Context.Data.CalibrationParametersProviderOfType<WeightsProvider>();

            _factorCorrelation = new double[factorCount, factorCount];

            for (int i = 0; i < factorCount; ++i)
            {
                for (int j = 0; j < factorCount; ++j)
                {
                    _factorCorrelation[i, j] = NMath.Dot(
                        weightsProvider.Value(factorIdentifiers[i]),
                        weightsProvider.Value(factorIdentifiers[j]));
                }
            }
        }

        protected IEnumerable<string> GetFactorIdentifiers()
        {
            return Enumerable.Range(0, factorCount).
                Select(i => string.Format("IR_DiscountFactor_{0}_Factor{1}", Identifier, i));
        }

        protected double Sigma(int factorIndex)
        {
            return _factors[factorIndex].Sigma;
        }

        protected double Lambda(int factorIndex)
        {
            return _factors[factorIndex].Lambda;
        }

        protected static double DriftHelper(double lambda, double t, double T)
        {
            return (1.0 / lambda) * (1 - Math.Exp(-lambda * t)) * (1 - Math.Exp(-lambda * T));
        }

        protected static double E(double lambda, double t)
        {
            return (1.0 - Math.Exp(-lambda * t)) / lambda;
        }

        protected double IntervalInYears(DateTime t1, DateTime t2)
        {
            return (t2 - t1).TotalDays / 365.25;
        }

        protected MeanRevertingNormalProcess[] _factors;
        protected MeanRevertingNormalPathModel[] _factorPaths;
        protected double[,] _factorCorrelation;
        protected int factorCount;
    }
}
