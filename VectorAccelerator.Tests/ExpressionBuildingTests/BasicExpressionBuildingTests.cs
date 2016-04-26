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
            var s0 = NArray.CreateScalar(100); // even if they are scalars, any independent variables should be NArrays
            
            var test = NMath.Exp(0.25 * state); // immediate executation mode outside Evaluate

            var result1 = NArray.Evaluate(() =>
            {
                var s = s0 * NMath.Exp(0.2 * state); // simple log-normal
                return s - k;
            }, s0);

            var expected = (s0 * NMath.Exp(0.2 * state) - k)
                .DebugDataView.ToArray();
            var obtained = result1[0].DebugDataView.ToArray();


            NArray.Evaluate(() =>
            {
                var s = s0 * NMath.Exp(0.2 * state); // simple log-normal
                return Finance.BlackScholes(CallPut.Call, s, k, 0.2, 1);
            }, s0);
        }
    }
}
