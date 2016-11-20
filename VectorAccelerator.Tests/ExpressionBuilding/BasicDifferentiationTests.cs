using System;
using System.Linq;
using NUnit.Framework;
using VectorAccelerator.Distributions;


namespace VectorAccelerator.Tests
{
    [TestFixture]
    public class BasicDifferentiationTests
    {
        [Test]
        public void BinaryOperatorTests()
        {
            NArray aVector, bVector;
            using (var stream = new RandomNumberStream(StorageLocation.Host))
            {
                aVector = NArray.CreateRandom(100, new Normal(stream, 0, 1));
                bVector = NArray.CreateRandom(100, new Normal(stream, 0, 1));
            }
            var aScalar = NArray.CreateScalar(aVector.First());
            var bScalar = NArray.CreateScalar(bVector.First());

            foreach (var operation in new string[] { "Add", "Subtract", "Multiply", "Divide" })
            {
                var vectorFunction = GetVectorFunction(operation);
                var scalarFunction = GetScalarFunction(operation);

                // we test all variations of having a and b being vectors and scalars
                foreach (var a in new NArray[] { aScalar, aVector })
                {
                    foreach (var b in new NArray[] {  bScalar, bVector })
                    {
                        // 1) test immediate mode operation
                        var vectorResult = vectorFunction(a, b);
                        var vectorViaScalarResult = VectorResultViaScalarFunction(a, b, scalarFunction);
                        Assert.IsTrue(TestHelpers.AgreesAbsolute(vectorResult.DebugDataView, vectorViaScalarResult));

                        // 2) test deferred mode operation
                        var deferredVectorResult = NArray.Evaluate(() =>
                        {
                            return vectorFunction(a, b);
                        });
                        Assert.IsTrue(TestHelpers.AgreesAbsoluteVectorLike(deferredVectorResult.DebugDataView, vectorViaScalarResult));

                        // 3) test deferred mode with differentiation
                        var deferredResult = NArray.Evaluate(() =>
                        {
                            return vectorFunction(a, b);
                        }, a, b);
                        Assert.IsTrue(TestHelpers.AgreesAbsoluteVectorLike(deferredResult[0].DebugDataView, vectorViaScalarResult));
                        // note that bumping makes use of vector operations, but it is tested separately that these are valid, so OK!
                        var aBumped = a + 1e-9;
                        var bBumped = b + 1e-9;
                        Assert.IsTrue(TestHelpers.AgreesAbsoluteVectorLike(deferredResult[1].DebugDataView,
                            ((vectorFunction(aBumped, b) - vectorFunction(a, b)) * 1e9).DebugDataView, 1e-4));
                        Assert.IsTrue(TestHelpers.AgreesAbsoluteVectorLike(deferredResult[2].DebugDataView,
                            ((vectorFunction(a, bBumped) - vectorFunction(a, b)) * 1e9).DebugDataView, 1e-4));
                    }
                }
            }
        }

        #region Operators To Test

        public static NArray Add(NArray a, NArray b)
        {
            return a + b;
        }

        public static double Add(double a, double b)
        {
            return a + b;
        }

        public static NArray Subtract(NArray a, NArray b)
        {
            return a - b;
        }

        public static double Subtract(double a, double b)
        {
            return a - b;
        }

        public static NArray Multiply(NArray a, NArray b)
        {
            return a * b;
        }

        public static double Multiply(double a, double b)
        {
            return a * b;
        }

        public static NArray Divide(NArray a, NArray b)
        {
            return a / b;
        }

        public static double Divide(double a, double b)
        {
            return a / b;
        }

        #endregion

        private double[] VectorResultViaScalarFunction(NArray a, NArray b, Func<double, double, double> function)
        {
            var result = new double[Math.Max(a.Length, b.Length)];
            var aArray = a.DebugDataView.ToArray();
            var bArray = b.DebugDataView.ToArray();
            for (int i = 0; i < result.Length; ++i)
            {
                result[i] = function(aArray.Length == 1 ? aArray[0] : aArray[i],
                    bArray.Length == 1 ? bArray[0] : bArray[i]);
            }
            return result;
        }

        private Func<NArray, NArray, NArray> GetVectorFunction(string functionName)
        {
            var method = this.GetType().GetMethod(functionName, new Type[] { typeof(NArray), typeof(NArray) });
            return (Func< NArray,NArray,NArray>)Delegate.CreateDelegate(typeof(Func<NArray, NArray, NArray>), method);
        }

        private Func<double, double, double> GetScalarFunction(string functionName)
        {
            var method = this.GetType().GetMethod(functionName, new Type[] { typeof(double), typeof(double) });
            return (Func<double, double, double>)Delegate.CreateDelegate(typeof(Func<double, double, double>), method);
        }

        [Test]
        public void DivisionExactTest()
        {
            NArray a, b;
            using (var stream = new RandomNumberStream(StorageLocation.Host))
            {
                a = NArray.CreateRandom(1000, new Normal(stream, 0, 1));
                b = NArray.CreateRandom(1000, new Normal(stream, 0, 1));
            }

            // First test
            var expected1 = 5.0 / a;
            var expectedDiff1 = -5.0 / (a * a);

            var obtained1 = NArray.Evaluate(() =>
            {
                return 5.0 / a;
            }, a);


            Assert.IsTrue(TestHelpers.AgreesAbsolute(obtained1[0], expected1));
            Assert.IsTrue(TestHelpers.AgreesAbsolute(obtained1[1], expectedDiff1));

            // Second test
            var expected2 = a / b;
            var expectedDiff2_1 = 1 / b;
            var expectedDiff2_2 = -a / (b * b);

            var obtained2 = NArray.Evaluate(() =>
            {
                return a / b;
            }, a, b);

            Assert.IsTrue(TestHelpers.AgreesAbsolute(obtained2[0], expected2));
            Assert.IsTrue(TestHelpers.AgreesAbsolute(obtained2[1], expectedDiff2_1));
            Assert.IsTrue(TestHelpers.AgreesAbsolute(obtained2[2], expectedDiff2_2));
        }
    }
}
