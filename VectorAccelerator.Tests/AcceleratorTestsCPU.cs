using System;
using System.Linq;
using VectorAccelerator;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VectorAccelerator.NArrayStorage;
using System.Threading.Tasks;
using VectorAccelerator.LinearAlgebraProviders;

namespace VectorAccelerator.Tests
{
    [TestClass]
    public class AcceleratorTestsCPU
    {        
        [TestMethod]
        public void SimpleSpeedTest()
        {
            int length = 1024 * 5;

            var a = NArray.CreateFromEnumerable(Enumerable.Range(0, length).Select(i => (double)i / length));
            var a2 = (a.Storage as VectorAccelerator.NArrayStorage.ManagedStorage<double>).Array;
            var b = NArray.CreateFromEnumerable(Enumerable.Range(3, length).Select(i => (double)i / length));
            var b2 = (b.Storage as VectorAccelerator.NArrayStorage.ManagedStorage<double>).Array;

            double[] result = null;
            NArray resultN = null;

            Console.WriteLine("Allocation");
            Timeit(
                () =>
                {
                    result = new double[length];
                });
            Console.WriteLine(Environment.NewLine);

            result = new double[length];
            Console.WriteLine("Managed optimal");
            Timeit(
                () =>
                {
                    for (int i = 0; i < result.Length; ++i)
                    //Parallel.For(0, result.Length, (i) =>
                    {
                        result[i] = Math.Log(Math.Exp(a2[i]) * Math.Exp(b2[i]));
                    }
                    //);
                });
            Console.WriteLine(Environment.NewLine);

            var vectorOptions = new VectorAccelerator.DeferredExecution.VectorExecutionOptions();
            vectorOptions.MultipleThreads = true;
            resultN = NArray.CreateLike(a);
            Console.WriteLine("Deferred threaded");
            Timeit(
                () =>
                {
                    using (NArray.DeferredExecution(vectorOptions))
                    {
                        resultN.Assign(NMath.Log(NMath.Exp(a) * NMath.Exp(b)));
                    }
                });
            Console.WriteLine(CheckitString(result, (resultN.Storage as ManagedStorage<double>).Array));
            Console.WriteLine(Environment.NewLine);

            resultN = NArray.CreateLike(a);
            vectorOptions.MultipleThreads = false;
            Console.WriteLine("Deferred not threaded");
            Timeit(
                () =>
                {
                    using (NArray.DeferredExecution(vectorOptions))
                    {
                        resultN.Assign(NMath.Log(NMath.Exp(a) * NMath.Exp(b)));
                    }
                });
            Console.WriteLine(CheckitString(result, (resultN.Storage as ManagedStorage<double>).Array));
            Console.WriteLine(Environment.NewLine);

            Console.WriteLine("Immediate");
            Timeit(
                () =>
                {
                    resultN.Assign(NMath.Log(NMath.Exp(a) * NMath.Exp(b)));
                });
            Console.WriteLine(CheckitString(result, (resultN.Storage as ManagedStorage<double>).Array));
            Console.WriteLine(Environment.NewLine);

            Console.WriteLine("Hit any key");
            Console.ReadKey();

            //double[] result2 = null;
            //Console.WriteLine("Managed vector");
            //Timeit(
            //    () =>
            //    {
            //        result = new double[length];
            //        for (int i = 0; i < result.Length; ++i)
            //        {
            //            result[i] = a2[i] * b2[i];
            //        }
            //        result2 = new double[length];
            //        for (int i = 0; i < result.Length; ++i)
            //        {
            //            result2[i] = 2 * result[i];
            //        }
            //    });
            //Console.WriteLine(result2.First());
            //Console.WriteLine(Environment.NewLine);
        }

        public static void Timeit(Action action, int repetitions = 10, int innerRepetitions = 10)
        {
            var watch = new Stopwatch();
            var ticks = new double[repetitions];
            var millisecs = new double[repetitions];
            watch.Start();
            for (int i = 0; i < repetitions; ++i)
            {
                for (int j = 0; j < innerRepetitions; ++j)
                {
                    action();
                }
                ticks[i] = (double)watch.ElapsedTicks / innerRepetitions;
                millisecs[i] = (double)watch.ElapsedMilliseconds / innerRepetitions;
                watch.Restart();
            }
            watch.Stop();
            Console.WriteLine(String.Format("Average time: {0} ticks", ticks.Skip(2).Average()));
            Array.Sort(ticks);
            Console.WriteLine(String.Format("Fastest time: {0} ticks", ticks.Min()));
            Console.WriteLine(String.Format("75 percentile fastest time: {0} ticks", ticks[(int)Math.Floor(repetitions * 0.75)]));
            Console.WriteLine(String.Format("Average time: {0} ms", millisecs.Skip(2).Average()));
        }

        public static string CheckitString(IList<double> first, IList<double> second)
        {
            return Checkit(first, second) ? "Matches" : "Does not match";
        }

        public static bool Checkit(IList<double> first, IList<double> second)
        {
            return (first.Count == second.Count) && !first.Zip(second, (f, s) => (f - s)).Any(d => Math.Abs(d) > 1e-6);
        }
    }
}
