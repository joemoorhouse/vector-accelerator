using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using VectorAccelerator.NArrayStorage;

namespace VectorAccelerator.Tests
{
    public static class TestHelpers
    {
        public static void Timeit(Action action, int repetitions = 10, int innerRepetitions = 50)
        {
            double dummy;
            Timeit(action, out dummy, repetitions, innerRepetitions);
        }
        
        public static void Timeit(Action action, out double averageTime, int repetitions = 10, int innerRepetitions = 50)
        {
            var watch = new Stopwatch();
            var tickMillisecs = new double[repetitions];
            var millisecs = new double[repetitions];
            watch.Start();
            for (int i = 0; i < repetitions; ++i)
            {
                for (int j = 0; j < innerRepetitions; ++j)
                {
                    action();
                }
                tickMillisecs[i] = (double)watch.ElapsedTicks * 1000 / (innerRepetitions * Stopwatch.Frequency);
                millisecs[i] = (double)watch.ElapsedMilliseconds / innerRepetitions;
                watch.Restart();
            }
            watch.Stop();
            if (repetitions == 1)
            {
                Console.WriteLine(String.Format("Average time: {0} ms", millisecs.First()));
                averageTime = millisecs.First();
            }
            else
            {
                averageTime = tickMillisecs.Skip(2).Average();
                Console.WriteLine(String.Format("Average time: {0} tick ms", averageTime));
                Array.Sort(tickMillisecs);
                Console.WriteLine(String.Format("Fastest time: {0} tick ms", tickMillisecs.Min()));
                Console.WriteLine(String.Format("75 percentile fastest time: {0} tick ms", tickMillisecs[(int)Math.Floor(repetitions * 0.75)]));
                Console.WriteLine(String.Format("Average time: {0} ms", millisecs.Skip(2).Average()));
            }
        }

        public static string AgreesAbsoluteString(NArray first, NArray second)
        {
            return AgreesAbsoluteString((first.Storage as ManagedStorage<double>).Array,
                (second.Storage as ManagedStorage<double>).Array);
        }

        public static string AgreesAbsoluteString(IList<double> first, IList<double> second)
        {
            return AgreesAbsolute(first, second) ? "Matches" : "Does not match";
        }

        public static bool AgreesAbsolute(NArray first, NArray second)
        {
            return AgreesAbsolute(first.DebugDataView, second.DebugDataView);
        }

        public static bool AgreesAbsolute(IEnumerable<double> first, IEnumerable<double> second, double tolerance = 1e-6)
        {
            if (first.Count() != second.Count()) return false;
            var diffs = first.Zip(second, (f, s) => (f - s)).Where(d => Math.Abs(d) > tolerance);
            return !diffs.Any();
        }
    }
}
