using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using VectorAccelerator.NArrayStorage;
using System.Text;

namespace VectorAccelerator.Tests
{
    public static class TestHelpers
    {
        public static void Timeit(Action action, int repetitions = 10, int innerRepetitions = 50)
        {
            double dummy;
            Timeit(action, out dummy, repetitions, innerRepetitions);
        }
        
        public static double TimeitSeconds(Action action)
        {
            var watch = new Stopwatch();
            watch.Start();
            action();
            watch.Stop();
            return (double)watch.ElapsedMilliseconds / 1000.0;
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
                Console.WriteLine(String.Format("Time: {0} ms", millisecs.First()));
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
            return AgreesAbsoluteString((first.Storage as ManagedStorage<double>).Data,
                (second.Storage as ManagedStorage<double>).Data);
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
            Func<double, double, bool> passes = (f, s) => Math.Abs(f - s) < tolerance;
            var diffs = first.Zip(second, (f, s) => new { First = f, Second = s }).Where(d => !passes(d.First, d.Second));
            if (diffs.Any())
            {
                var builder = new StringBuilder();
                builder.AppendLine(string.Format("Numbers of diffs: {0}", diffs.Count()));
                foreach (var item in diffs)
                {
                    builder.AppendLine(string.Format("{0:F7} versus {1:F7} ({2:E1})", item.First, item.Second, item.First - item.Second));
                }
                Console.WriteLine(builder.ToString());
            }
            return !diffs.Any();
        }

        /// <summary>
        /// Check for agreement, but interpret a constant vector to be equal to a scalar with the saem value.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool AgreesAbsoluteVectorLike(IEnumerable<double> first, IEnumerable<double> second, double tolerance = 1e-6)
        {
            int firstCount = first.Count();
            int secondCount = second.Count();
            if (firstCount != secondCount && (firstCount != 1 && secondCount != 1)) return false;
            int vectorLength = Math.Max(firstCount, secondCount);
            var firstToCompare = firstCount == 1 ? Enumerable.Repeat(first.First(), vectorLength) : first;
            var secondToCompare = secondCount == 1 ? Enumerable.Repeat(second.First(), vectorLength) : second;
            return AgreesAbsolute(firstToCompare, secondToCompare, tolerance);
        }
    }
}
