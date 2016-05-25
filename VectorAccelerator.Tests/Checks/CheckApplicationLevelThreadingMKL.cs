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

namespace VectorAccelerator.Tests.Checks
{
    /// <summary>
    /// Tests to check the approach of applying Application-level threading using MKL vector primitives 
    /// </summary>
    [TestClass]
    public class CheckApplicationLevelThreadingMKL
    {
        /// <summary>
        /// A test of MKL threading
        /// </summary>
        [TestMethod]
        public void TranscendentalFunctionTest()
        {
            IntelMathKernelLibrary.SetSequential();

            var location = StorageLocation.Host;

            using (var randomStream = new RandomNumberStream(location, RandomNumberGeneratorType.MRG32K3A, 111))
            {
                var normalDistribution = new Normal(randomStream, 0, 1);

                var a = new NArray(location, 5000);
                a.FillRandom(normalDistribution);

                var watch = new Stopwatch(); watch.Start();

                var options = new ParallelOptions() { MaxDegreeOfParallelism = 1 };

                Parallel.For(0, 100, options, (i) =>
                {
                    DoWork(a, normalDistribution);
                });

                var baseline = watch.ElapsedMilliseconds;
                Console.WriteLine("Base-line");
                Console.WriteLine(baseline);
                watch.Restart();

                Parallel.For(0, 100, (i) =>
                {
                    DoWork(a, normalDistribution);
                });

                var baselineThreaded = watch.ElapsedMilliseconds;
                Console.WriteLine("Base-line threaded");
                Console.WriteLine(baselineThreaded);
                watch.Restart();

                Parallel.For(0, 100, options, (i) =>
                    {
                        DoWorkInPlace(a, normalDistribution);
                    });

                var oneThread = watch.ElapsedMilliseconds;
                Console.WriteLine("In place");
                Console.WriteLine(oneThread); 
                watch.Restart();

                Parallel.For(0, 100, (i) =>
                    {
                        DoWorkInPlace(a, normalDistribution);
                    });

                var multipleThreads = watch.ElapsedMilliseconds;
                Console.WriteLine("In place threaded");
                Console.WriteLine(multipleThreads);
                Console.WriteLine(oneThread / (double)multipleThreads); 
                watch.Restart();

                Parallel.For(0, 100, options, (i) =>
                {
                    DoWorkDeferred(a, normalDistribution);
                });

                var deferred = watch.ElapsedMilliseconds;
                Console.WriteLine("Deferred");
                Console.WriteLine(deferred);
                watch.Restart();

                Parallel.For(0, 100, (i) =>
                {
                    DoWorkDeferred(a, normalDistribution);
                });

                var deferredMultipleThreads = watch.ElapsedMilliseconds;
                Console.WriteLine("Deferred threaded");
                Console.WriteLine(deferredMultipleThreads);
                watch.Restart();
            }
        }

        private void DoWork(NArray a, Normal normalDistribution)
        {
            // This will do compute-limited work that we would expect to task parallelize well
            var result = NArray.CreateLike(a);
            result.Assign(a);
            for (int j = 0; j < 100; ++j)
            {
                //result.FillNormal(random);

                var temp = NMath.Exp(result);
                result.Assign(NMath.Log(temp));
            }
        }

        private void DoWorkInPlace(NArray a, Normal normalDistribution)
        {
            // This will do compute-limited work that we would expect to task parallelize well
            var result = NArray.CreateLike(a);
            result.Assign(a);
            for (int j = 0; j < 100; ++j)
            {
                IntelMathKernelLibrary.Exp(GetArray(result), 0, GetArray(result), 0, result.Length);
                IntelMathKernelLibrary.Log(GetArray(result), 0, GetArray(result), 0, result.Length);
            }
        }

        private void DoWorkDeferred(NArray a, Normal normalDistribution, bool threaded = false)
        {
            // A version where assignment happens, but we defer execution.
            var result = NArray.CreateLike(a);
            result.Assign(a);

            var result2 = NArray.CreateLike(a);

            //var options = new DeferredExecution.VectorExecutionOptions() { MultipleThreads = threaded };

            for (int j = 0; j < 100; ++j)
            {
                NArray.Evaluate(() =>
                    {
                        return NMath.Log(NMath.Exp(result));
                    }, new List<NArray>(), Aggregator.ElementwiseAdd, new List<NArray> { result2 });
                
                //using (NArray.DeferredExecution(options))
                //{
                //    var temp = NMath.Exp(result);
                //    result.Assign(NMath.Log(temp));
                //}
            } 
        }

        private double[] GetArray(NArray a)
        {
            return (a.Storage as ManagedStorage<double>).Array;
        }

        /// <summary>
        /// Test that shows that as long as we do not have to create intermediate storage and this is not 
        /// too large, that vector operations do not have high overhead.
        /// </summary>
        [TestMethod]
        public void VectorFundamentalsTest()
        {
            var location = StorageLocation.Host;

            using (var randomStream = new RandomNumberStream(location, RandomNumberGeneratorType.MRG32K3A, 111))
            {
                var normalDistribution = new Normal(randomStream, 0, 1);

                var a = new NArray(StorageLocation.Host, 5000);
                var b = new NArray(StorageLocation.Host, 5000);
                var c = new NArray(StorageLocation.Host, 5000);
                var d = new NArray(StorageLocation.Host, 5000);
                var result = NArray.CreateLike(a);
                a.FillRandom(normalDistribution);
                b.FillRandom(normalDistribution);
                c.FillRandom(normalDistribution);
                d.FillRandom(normalDistribution);
                var aArray = GetArray(a);
                var bArray = GetArray(b);
                var cArray = GetArray(c);
                var dArray = GetArray(d);
                var resultArray = GetArray(result);

                Console.WriteLine(); Console.WriteLine("In place vector Exp MKL");
                TestHelpers.Timeit(() =>
                    {
                        IntelMathKernelLibrary.Exp(aArray, 0, resultArray, 0, result.Length);
                    }, 20, 50);

                Console.WriteLine(); Console.WriteLine("In place vector Exp C sharp");
                TestHelpers.Timeit(() =>
                {
                    for (int i = 0; i < 5000; ++i)
                    {
                        resultArray[i] = Math.Exp(aArray[i]);
                    }
                }, 20, 50);

                Console.WriteLine(); Console.WriteLine("In place vector aX + Y MKL");
                TestHelpers.Timeit(() =>
                {
                    IntelMathKernelLibrary.ConstantAddMultiply(aArray, 0, 3, 0, resultArray, 0, 5000);
                    //IntelMathKernelLibrary.Multiply(bArray, 0, resultArray, 0, resultArray, 0, result.Length);
                    IntelMathKernelLibrary.Add(bArray, 0, resultArray, 0, resultArray, 0, result.Length);
                    //IntelMathKernelLibrary.Exp(resultArray, 0, resultArray, 0, result.Length);
                    // 3 reads and 2 writes
                }, 20, 50);

                Console.WriteLine(); Console.WriteLine("In place vector aX + Y C sharp");
                TestHelpers.Timeit(() =>
                {
                    for (int i = 0; i < 5000; ++i)
                    {
                        resultArray[i] = 3 * aArray[i] + bArray[i]; // 2 reads and a write
                        //resultArray[i] = Math.Exp(3 * aArray[i] + bArray[i]); // 2 reads and a write
                    }
                }, 20, 50);

                Console.WriteLine(); Console.WriteLine("Immediate mode; creating storage");
                TestHelpers.Timeit(() =>
                {
                    //var result2 = NMath.Exp(3 * a + b);
                    var result2 = 3 * a + b;
                }, 20, 50);

                var result3 = NArray.CreateLike(a);

                Console.WriteLine(); Console.WriteLine("Deferred mode; storage passed in");
                TestHelpers.Timeit(() =>
                {
                    NArray.Evaluate(() =>
                        {
                            //return NMath.Exp(3 * a + b);
                            return 3 * a + b;
                        }
                        , new List<NArray>(), Aggregator.ElementwiseAdd, new List<NArray> { result3 });
                }, 20, 50);

                return;

                Console.WriteLine(); Console.WriteLine("Deferred mode; storage passed in");
                TestHelpers.Timeit(() =>
                {
                    NArray.Evaluate(() =>
                    {
                        return 3 * a + b;
                    }
                        , new List<NArray>(), Aggregator.ElementwiseAdd, new List<NArray> { result });
                }, 20, 50);
            }
        }
    }
}
