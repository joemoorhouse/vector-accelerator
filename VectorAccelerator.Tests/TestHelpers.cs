using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using VectorAccelerator.NArrayStorage;
using VectorAccelerator.LinearAlgebraProviders;

namespace VectorAccelerator.Tests
{
    public static class TestHelpers
    {
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

        public static string CheckitString(NArray first, NArray second)
        {
            return CheckitString((first.Storage as ManagedStorage<double>).Array,
                (second.Storage as ManagedStorage<double>).Array);
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
