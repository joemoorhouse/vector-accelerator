using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using VectorAccelerator.Distributions;

namespace VectorAccelerator.Tests
{
    [TestFixture]
    public class PresentationTests
    {
        [Test]
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

            // can also call in such a way that the expression list is appended to a StringBuilder:
            var logger = new StringBuilder();
            var loggedResult = NArray.Evaluate(() =>
            {
                return x0 * x1 + x0 * NMath.Exp(2 * x1);
            }, logger, x0, x1);

            Console.WriteLine(logger.ToString());
        }

        [Test]
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

        [Test]
        public void BlackScholesPerformance()
        {
            double k = 90;
            var volatility = 0.2;
            var t = 5;

            var s = new List<NArray>();
            int batchCount = 1000;

            using (var stream = new RandomNumberStream(StorageLocation.Host, RandomNumberGeneratorType.MRG32K3A))
            {
                var normal = new Normal(stream, 0, 1);
                for (int i = 0; i < batchCount; ++i)
                {
                    s.Add(100 * NMath.Exp(NArray.CreateRandom(5000, normal) * 0.2));
                }
            }

            var result = NArray.CreateLike(s.First());

            double elapsedTime = TestHelpers.TimeitSeconds(() =>
            {
                for (int i = 0; i < batchCount; ++i)
                {
                    NArray.Evaluate(() =>
                    {
                        return Finance.BlackScholes(CallPut.Call, s[i], k, volatility, t);
                    }, new List<NArray>(), Aggregator.ElementwiseAdd, new List<NArray> { result });
                }
            });

            Console.WriteLine(string.Format("Time per option price: {0} ns", elapsedTime * 1e9 / (batchCount * 5000)));
            Console.WriteLine(string.Format("Valuations per second: {0:F0} million", batchCount * 5000 / (elapsedTime * 1e6)));
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
