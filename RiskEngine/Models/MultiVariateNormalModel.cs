using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator;
using VectorAccelerator.Distributions;
using RiskEngine.Framework;
using RiskEngine.Data;

namespace RiskEngine.Models
{
    /// <summary>
    /// Model that produces random variates sampled from a multivariate normal distribution
    /// </summary>
    public class MultiVariateNormalModel : MultiFactorProcess
    {
        public override void Initialise(Simulation simulation)
        {
            var randomStream = new RandomNumberStream(simulation.Context.Factory.StorageLocation,
                RandomNumberGeneratorType.MRG32K3A, 111);
            _normalDistribution = new Normal(randomStream, 0, 1);
        }

        internal override void Prepare(Context context)
        {
            var weightsProvider = context.Data.CalibrationParametersProviderOfType<WeightsProvider>();
            
            // we want to end up with the variates for each factor in the columns, because matrix is column-major which
            // means that storage for a factor is then contiguous
            _correlated = context.Factory.CreateNArray(context.Settings.SimulationCount, _factors.Count);
            _uncorrelated = context.Factory.CreateNArray(context.Settings.SimulationCount, weightsProvider.WeightsLength);
            _rootCovariance = context.Factory.CreateNArray(weightsProvider.WeightsLength, _factors.Count);
            
            for (int i = 0; i < _factors.Count; ++i)
            {
                _rootCovariance.SetColumn(i, weightsProvider.Value(_factors[i].Identifier));

                _factors[i].Value = _correlated.ColumnAsReference(i);
            }
        }

        protected override void Step(TimeInterval timeStep)
        {
            // do not care about timeStep: batches are simply normal variates
            _uncorrelated.FillRandom(_normalDistribution);
            NMath.MatrixMultiply(_uncorrelated, _rootCovariance, _correlated);
        }

        public void Dispose()
        {
            _normalDistribution.RandomNumberStream.Dispose();
        }

        private Normal _normalDistribution;
        private NArray _rootCovariance;
        private NArray _uncorrelated;
        private NArray _correlated;

        public class WeightsProvider : ParameterProvider<NArray> 
        {   
            public readonly int WeightsLength; 
            
            public WeightsProvider(int weightsLength)
            {
                WeightsLength = weightsLength;
            }

            public override void AddValue(string factor, NArray weights)
            {
                Assertions.AssertIsVectorOfLength(weights, WeightsLength, "weights");
                if (weights.Length != WeightsLength) throw new ArgumentException("length mismatch");
                _cache[factor] = weights;
            }
        }
    }
}
