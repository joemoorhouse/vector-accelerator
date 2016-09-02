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
    [TestClass]
    public class AcceleratorTestsCPU
    {        
        [TestMethod]
        public void SimpleSpeedTest()
        {
            var factory = new NArrayFactory(StorageLocation.Host);
            
            IntelMathKernelLibrary.SetAccuracyMode(VMLAccuracy.LowAccuracy);
            
            int length = 1024 * 5;

            var a = factory.CreateFromEnumerable(Enumerable.Range(0, length).Select(i => (double)i / length));
            var a2 = (a.Storage as VectorAccelerator.NArrayStorage.ManagedStorage<double>).Array;
            var b = factory.CreateFromEnumerable(Enumerable.Range(3, length).Select(i => (double)i / length));
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
                        result[i] = 3 * Math.Log(Math.Exp(a2[i]) * Math.Exp(b2[i]));
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
                    NArray.Evaluate(vectorOptions, () =>
                    {
                        return 3 * NMath.Log(NMath.Exp(a) * NMath.Exp(b));
                    });
                });
            Console.WriteLine(CheckitString(result, (resultN.Storage as ManagedStorage<double>).Array));
            Console.WriteLine(Environment.NewLine);

            resultN = NArray.CreateLike(a);
            vectorOptions.MultipleThreads = false;
            Console.WriteLine("Deferred not threaded");
            Timeit(
                () =>
                {
                    NArray.Evaluate(vectorOptions, () =>
                    {
                        return 3 * NMath.Log(NMath.Exp(a) * NMath.Exp(b));
                    });
                });
            Console.WriteLine(CheckitString(result, (resultN.Storage as ManagedStorage<double>).Array));
            Console.WriteLine(Environment.NewLine);

            Console.WriteLine("Immediate");
            Timeit(
                () =>
                {
                    resultN.Assign(3 * NMath.Log(NMath.Exp(a) * NMath.Exp(b)));
                });
            Console.WriteLine(CheckitString(result, (resultN.Storage as ManagedStorage<double>).Array));
            Console.WriteLine(Environment.NewLine);

            Console.WriteLine("Hit any key");
            Console.ReadKey();
        }

        [TestMethod]
        public void TestBlackScholes()
        {
            var location = StorageLocation.Host;

            using (var randomStream = new RandomNumberStream(location, RandomNumberGeneratorType.MRG32K3A, 111))
            {
                var normalDistribution = new Normal(randomStream, 0, 1);
                
                double deltaT = 1;
                double r = 0.1;
                double vol = 0.3;

                var options = new ParallelOptions();
                options.MaxDegreeOfParallelism = 1;

                var variates = NArray.CreateRandom(5000, normalDistribution);
                NArray optionPrices = NArray.CreateLike(variates);
                var watch = new Stopwatch(); watch.Start();
                
                Parallel.For(0, 1000, options, (i) =>
                {
                    optionPrices = NArray.CreateLike(variates);
                    var logStockPrice = Math.Log(100)
                        + variates * Math.Sqrt(deltaT) * vol + (r - 0.5 * vol * vol) * deltaT;
                    var stockPrices = NMath.Exp(logStockPrice);

                    optionPrices.Assign(Finance.BlackScholes(-1, stockPrices, 90, 0.2, 1));
                });
                Console.WriteLine("Baseline sequential");
                Console.WriteLine(watch.ElapsedMilliseconds); watch.Restart();

                Parallel.For(0, 1000, (i) =>
                {
                    optionPrices = NArray.CreateLike(variates);
                    var logStockPrice = Math.Log(100)
                        + variates * Math.Sqrt(deltaT) * vol + (r - 0.5 * vol * vol) * deltaT;
                    var stockPrices = NMath.Exp(logStockPrice);

                    optionPrices.Assign(Finance.BlackScholes(-1, stockPrices, 90, 0.2, 1));
                });
                Console.WriteLine("Baseline threaded");
                Console.WriteLine(watch.ElapsedMilliseconds); 

                NArray optionPrices2 = NArray.CreateLike(variates);

                watch.Restart();
                var vectorOptions = new DeferredExecution.VectorExecutionOptions() { MultipleThreads = true };
                Parallel.For(0, 1000, options, (i) =>
                {
                    optionPrices2 = NArray.CreateLike(variates);
                    NArray.Evaluate(optionPrices2, () =>
                    {
                        var logStockPrice = Math.Log(100)
                            + variates * Math.Sqrt(deltaT) * vol + (r - 0.5 * vol * vol) * deltaT;
                        var stockPrices = NMath.Exp(logStockPrice);

                        return Finance.BlackScholes(-1, stockPrices, 90, 0.2, 1);
                    });
                });
                Console.WriteLine("Deferred sequential");
                Console.WriteLine(watch.ElapsedMilliseconds);
                Console.WriteLine(CheckitString((optionPrices.Storage as ManagedStorage<double>).Array, (optionPrices2.Storage as ManagedStorage<double>).Array));
                watch.Restart();

                Parallel.For(0, 1000, (i) =>
                {
                    //optionPrices2 = NArray.CreateLike(variates);
                    optionPrices2 = NArray.Evaluate(() =>
                    {
                        var logStockPrice = Math.Log(100)
                            + variates * Math.Sqrt(deltaT) * vol + (r - 0.5 * vol * vol) * deltaT;
                        var stockPrices = NMath.Exp(logStockPrice);

                        return Finance.BlackScholes(-1, stockPrices, 90, 0.2, 1);
                    });
                });
                Console.WriteLine("Deferred threaded");
                Console.WriteLine(watch.ElapsedMilliseconds); watch.Restart();
                Console.WriteLine(CheckitString((optionPrices.Storage as ManagedStorage<double>).Array, (optionPrices2.Storage as ManagedStorage<double>).Array));
            }
        }

        public static void Timeit(Action action, int repetitions = 10, int innerRepetitions = 50)
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
