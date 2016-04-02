using System;
using System.Linq;
using VectorAccelerator;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VectorAccelerator.NArrayStorage;
using System.Threading.Tasks;
using VectorAccelerator.Distributions;
using VectorAccelerator.LinearAlgebraProviders;

namespace VectorAccelerator.Tests
{
    public class BasicExpressionBuildingTests
    {
        public void CUDA()
        {
            var location = StorageLocation.Host;
            var normal = new Normal(new RandomNumberStream(location), 0, 1);
            var input = NArray.CreateRandom(1000, normal);
            var result = NArray.CreateLike(input);
            using (NArray.DeferredExecution())
            {
                var temp = NMath.Exp(input * 0.2 + 6);
                result.Assign(temp);
            }
        }

        /// <summary>
        /// Adjoint algorithmic differentiation test
        /// </summary>
        public void AAD()
        {
            var location = StorageLocation.Host;
            var normal = new Normal(new RandomNumberStream(location), 0, 1);
            var state = NArray.CreateRandom(1000, normal);
            var k = 95;
            var result = NArray.CreateLike(state);
            var s0 = NArray.CreateScalar(100);
            s0.IsIndependentVariable = true;
            using (NArray.DeferredExecution())
            {
                var s = s0 * NMath.Exp(0.2 * state); // simple log-normal
                result.Assign(s - k); // non-discounting forward
            }

            Evaluate(() =>
                {
                    var s = s0 * NMath.Exp(0.2 * state); // simple log-normal
                    return s - k;
                }, s0);
        }

        public void Evaluate(Func<NArray> function, params NArray[] independentVariables)
        {

        }
    }
}
