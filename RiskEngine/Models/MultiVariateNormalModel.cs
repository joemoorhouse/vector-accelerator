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
    public class MultiVariateNormalModel : MultiFactorModel
    {
        public MultiVariateNormalModel(NArrayFactory factory)
        {
            _factory = factory;
            var randomStream = new RandomNumberStream(factory.StorageLocation,
                RandomNumberGeneratorType.MRG32K3A, 111);
            _normalDistribution = new Normal(randomStream, 0, 1);
        }

        public override void Prepare(Context context)
        {
            var weightsProvider = context.Data.CalibrationParametersProviderOfType<WeightsProvider>();
            
            // we want to end up with the variates for each factor in the columns, because matrix is column-major which
            // means that storage for a factor is then contiguous
            _correlated = _factory.CreateNArray(context.Settings.SimulationCount, _factors.Count); 
            _uncorrelated = _factory.CreateNArray(context.Settings.SimulationCount, weightsProvider.WeightsLength);
            _rootCovariance = _factory.CreateNArray(weightsProvider.WeightsLength, _factors.Count);
            
            for (int i = 0; i < _factors.Count; ++i)
            {
                _rootCovariance.SetColumn(i, _factory.CreateNArray(
                    weightsProvider.Value(_factors[i].Identifier)));

                _factors[i].Value = _correlated.ColumnAsReference(i);
            }
        }

        public override void Step(TimeInterval timeStep)
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

        class WeightsProvider : ParameterProvider<double[]> 
        {   
            public readonly int WeightsLength; 
            
            public WeightsProvider(int weightsLength)
            {
                WeightsLength = weightsLength;
            }

            public override void AddValue(string factor, double[] weights)
            {
                if (weights.Length != WeightsLength) throw new ArgumentException("length mismatch");
                _cache[factor] = weights;
            }
        }
    }
}
