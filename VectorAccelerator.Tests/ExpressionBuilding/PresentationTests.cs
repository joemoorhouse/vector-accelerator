using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VectorAccelerator.Distributions;

namespace VectorAccelerator.Tests
{
    [TestClass]
    public class PresentationTests
    {
        [TestMethod]
        public void WorkedExample()
        {
            NArray x0, x1;
            using (var stream = new RandomNumberStream(StorageLocation.Host, RandomNumberGeneratorType.MRG32K3A))
            {
                var normal = new Normal(stream, 0, 1);
                x0 = NArray.CreateRandom(5000, normal);
                x1 = NArray.CreateRandom(5000, normal);
            }

            var result = NArray.Evaluate(() =>
            {
                return x0 * x1 + x0 * NMath.Exp(2 * x1);
            }, x0, x1);

            var derivativeWRTx0 = result[1];
            var derivativeWRTx1 = result[2];

            Assert.IsTrue(TestHelpers.AgreesAbsolute(derivativeWRTx0, x1 + NMath.Exp(2 * x1)));
            Assert.IsTrue(TestHelpers.AgreesAbsolute(derivativeWRTx1, x0 + 2 * x0 * NMath.Exp(2 * x1)));
        }

        [TestMethod]
        public void BlackScholes()
        {
            NArray s;
            using (var stream = new RandomNumberStream(StorageLocation.Host, RandomNumberGeneratorType.MRG32K3A))
            {
                var normal = new Normal(stream, 0, 1);
                s = 100 * NMath.Exp(NArray.CreateRandom(5, normal) * 0.2);
            }

            double k = 90;
            var r = 0.005;
            var volatility = 0.2;
            var t = 5;

            var result = NArray.Evaluate(() =>
            {
                var f = s * Math.Exp(r * t); 
                return Finance.BlackScholes(CallPut.Call, f, k, volatility, t);
            }, s);

            var sds = s + 1e-6;
            var check = (Finance.BlackScholes(CallPut.Call, sds * Math.Exp(r * t), k, volatility, t)
                - Finance.BlackScholes(CallPut.Call, s * Math.Exp(r * t), k, volatility, t)) * 1e6;

            Assert.IsTrue(TestHelpers.AgreesAbsolute(result[1], check));
        }

        public void CUDA()
        {
            var location = StorageLocation.Host;
            var normal = new Normal(new RandomNumberStream(location), 0, 1);
            var input = NArray.CreateRandom(1000, normal);
            var result = NArray.CreateLike(input);
            NArray.Evaluate(() =>
            {
                return NMath.Exp(input * 0.2 + 6);
            });
        }
    }
}
