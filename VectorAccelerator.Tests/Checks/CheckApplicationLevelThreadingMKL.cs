using System;
using System.Linq;
using VectorAccelerator;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VectorAccelerator.NArrayStorage;
using System.Threading.Tasks;
using VectorAccelerator.LinearAlgebraProviders;

namespace VectorAccelerator.Tests.Checks
{
    /// <summary>
    /// Tests to check the approach of applying Application-level threading using MKL vector primitives 
    /// </summary>
    [TestClass]
    public class CheckApplicationLevelThreadingMKL
    {
        [TestMethod]
        public void CumulativeNormalTest()
        {
            IntelMathKernalLibrary.SetSequential();
            
            using (var random = new IntelMKLRandomNumberGenerator(RandomNumberGeneratorType.MRG32K3A, 111))
            {
                var a = new NArray(5000);
                a.FillNormal(random);

                var watch = new Stopwatch(); watch.Start();

                var options = new ParallelOptions() { MaxDegreeOfParallelism = 1 };

                Parallel.For(0, 100, options, (i) =>
                {
                    DoWork(a, random);
                });

                var baseline = watch.ElapsedMilliseconds;
                Console.WriteLine("Base-line");
                Console.WriteLine(baseline);
                watch.Restart();

                Parallel.For(0, 100, (i) =>
                {
                    DoWork(a, random);
                });

                var baselineThreaded = watch.ElapsedMilliseconds;
                Console.WriteLine("Base-line threaded");
                Console.WriteLine(baselineThreaded);
                watch.Restart();

                Parallel.For(0, 100, options, (i) =>
                    {
                        DoWorkInPlace(a, random);
                    });

                var oneThread = watch.ElapsedMilliseconds;
                Console.WriteLine("In place");
                Console.WriteLine(oneThread); 
                watch.Restart();

                Parallel.For(0, 100, (i) =>
                    {
                        DoWorkInPlace(a, random);
                    });

                var multipleThreads = watch.ElapsedMilliseconds;
                Console.WriteLine("In place threaded");
                Console.WriteLine(multipleThreads);
                Console.WriteLine(oneThread / (double)multipleThreads); 
                watch.Restart();

                Parallel.For(0, 100, options, (i) =>
                {
                    DoWorkDeferred(a, random);
                });

                var deferred = watch.ElapsedMilliseconds;
                Console.WriteLine("Deferred");
                Console.WriteLine(deferred);
                watch.Restart();

                Parallel.For(0, 100, (i) =>
                {
                    DoWorkDeferred(a, random);
                });

                var deferredMultipleThreads = watch.ElapsedMilliseconds;
                Console.WriteLine("Deferred threaded");
                Console.WriteLine(deferredMultipleThreads);
                watch.Restart();
            }
        }

        private void DoWork(NArray a, IRandomNumberGenerator random)
        {
            // This will do compute limited work that we would expect to task parallelize well
            var result = NArray.CreateLike(a);
            result.Assign(a);
            for (int j = 0; j < 100; ++j)
            {
                //result.FillNormal(random);
                var temp = NMath.Exp(result);
                result.Assign(NMath.Log(temp));
            }
        }

        private void DoWorkInPlace(NArray a, IRandomNumberGenerator random)
        {
            // This will do compute limited work that we would expect to task parallelize well
            var result = NArray.CreateLike(a);
            result.Assign(a);
            for (int j = 0; j < 100; ++j)
            {
                //result.FillNormal(random);
                IntelMathKernalLibrary.Exp(GetArray(result), 0, GetArray(result), 0, result.Length);
                IntelMathKernalLibrary.Log(GetArray(result), 0, GetArray(result), 0, result.Length);
            }
        }

        private void DoWorkDeferred(NArray a, IRandomNumberGenerator random, bool threaded = false)
        {
            // A version where assignment happens, but we defer execution.
            var result = NArray.CreateLike(a);
            result.Assign(a);

            var options = new DeferredExecution.VectorExecutionOptions() { MultipleThreads = threaded };

            for (int j = 0; j < 100; ++j)
            {
                using (NArray.DeferredExecution(options))
                {
                    //result.FillNormal(random);
                    var temp = NMath.Exp(result);
                    result.Assign(NMath.Log(temp));
                }
            } 
        }

        private double[] GetArray(NArray a)
        {
            return (a.Storage as ManagedStorage<double>).Array;
        }
    }
}
