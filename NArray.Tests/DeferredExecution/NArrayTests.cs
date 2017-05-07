using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NArray.LinearAlgebraProviders;

namespace NArray
{
    [TestFixture]
    public class NArrayTests
    {
        [Test]
        public static void TestBasic()
        {
            var a = new NArray(5000, 1);
            var b = new NArray(5000, 1);
            var c = new NArray(5000, 1);

            using (var stream = new IntelMKLRandomNumberStream(RandomNumberGeneratorType.MRG32K3A, 111))
            {
                IntelMathKernelLibraryRandom.FillNormals(a.Storage.Data, stream);
                IntelMathKernelLibraryRandom.FillNormals(b.Storage.Data, stream);
                IntelMathKernelLibraryRandom.FillNormals(c.Storage.Data, stream);
            }
            var d = a + b;

            var result = NArrayDeferred.Evaluate(() =>
            {
                return a + b;
            }, new List<NArray> { a });

            var result2 = NArrayDeferred.Evaluate(() =>
            {
                return a + b;
            }, new List<NArray> { a });
        }

        [Test]
        public static void WorkedExample()
        {
            var x0 = new NArray(5000, 1);
            var x1 = new NArray(5000, 1);
            using (var stream = new IntelMKLRandomNumberStream(RandomNumberGeneratorType.MRG32K3A, 111))
            {
                IntelMathKernelLibraryRandom.FillNormals(x0.Storage.Data, stream);
                IntelMathKernelLibraryRandom.FillNormals(x1.Storage.Data, stream);
            }

            TestHelpers.Timeit(() =>
            {
                var resultSingleTemp = NArray.Evaluate(() =>
                {
                    return x0 * x1 + x0 * NMath.Exp(2 * x1);
                });
            });
            
            var resultSingle = NArray.Evaluate(() =>
            {
                return x0 * x1 + x0 * NMath.Exp(2 * x1);
            });

            resultSingle = NArray.Evaluate(() =>
            {
                return x0 * x1 + x0 * NMath.Exp(2 * x1);
            });

            resultSingle = NArray.Evaluate(() =>
            {
                return x0 * x1 + x0 * NMath.Exp(2 * x1);
            });

            var result = NArray.Evaluate(() =>
            {
                return x0 * x1 + x0 * NMath.Exp(2 * x1);
            }, x0, x1);

            var derivativeWRTx0 = result[1];
            var derivativeWRTx1 = result[2];

            AssertIsTrue(TestHelpers.AgreesAbsolute(derivativeWRTx0, x1 + NMath.Exp(2 * x1)));
            AssertIsTrue(TestHelpers.AgreesAbsolute(derivativeWRTx1, x0 + 2 * x0 * NMath.Exp(2 * x1)));

            // can also call in such a way that the expression list is appended to a StringBuilder:
            var logger = new StringBuilder();
            var loggedResult = NArray.Evaluate(() =>
            {
                return x0 * x1 + x0 * NMath.Exp(2 * x1);
            }, logger, x0, x1);

            Console.WriteLine(logger.ToString());
        }

        [Test]
        public static void BlackScholes()
        {
            var simpleCheck = Math.Exp(-0.1 * 0.5) * Finance.BlackScholes(
                CallPut.Call, 42 * Math.Exp(0.1 * 0.5), 40, 0.2, 0.5);

            AssertIsTrue(TestHelpers.AgreesAbsolute(simpleCheck, (NArray)4.75942239));

            var normal = new NArray(5000, 1);
            using (var stream = new IntelMKLRandomNumberStream(RandomNumberGeneratorType.MRG32K3A, 111))
            {
                IntelMathKernelLibraryRandom.FillNormals(normal.Storage.Data, stream);
            }

            var s = 100 * NMath.Exp(normal * 0.2);

            double k = 90;
            var volatility = 0.2;
            var t = 5;
            var df = Math.Exp(-0.005 * t);

            var result = NArray.Evaluate(() =>
            {
                var f = s / df;
                return df * Finance.BlackScholes(CallPut.Call, f, k, volatility, t);
            }, s);

            var sds = s + 1e-6;
            var check = (df * Finance.BlackScholes(CallPut.Call, sds / df, k, volatility, t)
                - df * Finance.BlackScholes(CallPut.Call, s / df, k, volatility, t)) * 1e6;

            AssertIsTrue(TestHelpers.AgreesAbsolute(result[1], check));
        }

        public static void AssertIsTrue(bool condition)
        {
            if (!condition) throw new Exception();
        }

        public enum CallPut { Call, Put }

        public class Finance
        {
            public static NArray BlackScholes(CallPut callPutFactor, NArray forward, double strike,
                NArray volatility, double deltaTime)
            {
                return BlackScholes(callPutFactor == CallPut.Call ? 1 : -1, forward, strike, volatility, deltaTime);
            }

            public static NArray BlackScholes(double callPutFactor, NArray forward, double strike,
                NArray volatility, double deltaTime)
            {
                NArray d1, d2;
                if (deltaTime == 0) return callPutFactor * (forward - strike);

                BlackScholesD1D2Parameters(forward, strike, volatility, deltaTime, out d1, out d2);

                var nd1 = NMath.CumulativeNormal(callPutFactor * d1);
                var nd2 = NMath.CumulativeNormal(callPutFactor * d2);

                return callPutFactor * (forward * nd1 - strike * nd2);
            }

            public static void BlackScholesD1D2Parameters(NArray forward, double strike, NArray volatility,
                double deltaTime, out NArray d1, out NArray d2)
            {
                var volMultSqrtDTime = volatility * Math.Sqrt(deltaTime);

                d1 = (NMath.Log(forward / strike) + (0.5 * volatility * volatility * deltaTime))
                    / volMultSqrtDTime;

                d2 = d1 - volMultSqrtDTime;
            }
        }

        public static void TestBoolPerformance()
        {
            double[] test1 = new double[5000];
            double[] test2 = new double[5000];

            using (var stream = new IntelMKLRandomNumberStream(RandomNumberGeneratorType.MRG32K3A, 111))
            {
                IntelMathKernelLibraryRandom.FillNormals(test1, stream);
                IntelMathKernelLibraryRandom.FillNormals(test2, stream);
            }

            var doubleBool1 = test1.Select(v => v < 0 ? 1.0 : 0.0).ToArray();
            var doubleBool2 = test2.Select(v => v < 0 ? 1.0 : 0.0).ToArray();
            var doubleBoolResult = test2.Select(v => 0.0).ToArray();

            var boolBool1 = test1.Select(v => v < 0).ToArray();
            var boolBool2 = test2.Select(v => v < 0).ToArray();
            var boolBoolResult = test2.Select(v => false).ToArray();

            TestHelpers.Timeit(
                () =>
                {
                    for (int i = 0; i < doubleBoolResult.Length; ++i)
                    {
                        doubleBoolResult[i] = (doubleBool1[i] == 1.0) && (doubleBool2[i] == 1.0) ? 1.0 : 0.0;
                    }

                });

            TestHelpers.Timeit(
                () =>
                {
                    for (int i = 0; i < doubleBoolResult.Length; ++i)
                    {
                        boolBoolResult[i] = boolBool1[i] && boolBool2[i];
                    }

                });
        }
    }
}
