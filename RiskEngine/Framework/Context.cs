using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskEngine.Data;
using VectorAccelerator;

namespace RiskEngine.Framework
{
    public class Context
    {
        public readonly DataProvider Data = new DataProvider();

        public readonly NArrayFactory Factory;

        public readonly SimulationSettings Settings;

        public Context(StorageLocation location, DateTime closeOfBusinessDate)
        {
            Factory = new NArrayFactory(location);
            Settings = new SimulationSettings(closeOfBusinessDate);
        }
    }

    public class SimulationSettings
    {
        public int SimulationCount { get; set; }

        public TimeInterval[] SimulationIntervals { get; private set; }
        
        public TimePoint[] SimulationTimePoints { get; private set; }

        public SimulationSettings(DateTime closeOfBusinessDate)
        {
            SimulationCount = 5000;
            //var simulationDateTimes = SimulationDateTimes(closeOfBusinessDate).ToList();

            var simulationDateTimes = AddLastMarginCallAndCloseoutPoints(ReportingPoints())
                .Select(i => closeOfBusinessDate.AddDays(i))
                .ToList();

            SimulationIntervals = simulationDateTimes.Skip(1)
                .Zip(simulationDateTimes, (n, p) => new TimeInterval(p, n))
                .ToArray();

            SimulationTimePoints = simulationDateTimes
                .Select(p => new TimePoint(p, (p - simulationDateTimes[0]).Days / 365.25))
                .ToArray();
        }

        /// <summary>
        /// One choice of points
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        private IEnumerable<DateTime> SimulationDateTimes(DateTime start)
        {
            var startDateTime = start;
            var startWeekly = startDateTime.AddMonths(1);
            var startMonthly = startDateTime.AddYears(2);
            var startAnnual = startDateTime.AddYears(5);
            var start5Yearly = startDateTime.AddYears(20); // 5
            var end = startDateTime.AddYears(60);

            var currentDateTime = startDateTime;
            while (currentDateTime < startWeekly)
            {
                yield return currentDateTime;
                currentDateTime = currentDateTime.AddDays(1);
            }
            
            currentDateTime = startWeekly;
            while (currentDateTime < startMonthly)
            {
                yield return currentDateTime;
                currentDateTime = currentDateTime.AddDays(7);
            }

            currentDateTime = startMonthly;
            while (currentDateTime < startAnnual)
            {
                yield return currentDateTime;
                currentDateTime = currentDateTime.AddMonths(1);
            }

            currentDateTime = startAnnual;
            while (currentDateTime < start5Yearly)
            {
                yield return currentDateTime;
                currentDateTime = currentDateTime.AddYears(1);
            }

            currentDateTime = start5Yearly;
            while (currentDateTime <= end)
            {
                yield return currentDateTime;
                currentDateTime = currentDateTime.AddYears(5);
            }
        }

        private static IEnumerable<int> AddLastMarginCallAndCloseoutPoints(IEnumerable<int> reportingSet)
        {
            var reporting = reportingSet.ToArray();
            foreach (var item in Enumerable.Range(0, 28)) yield return item;
            for (int i = 16; i < reporting.Length; ++i)
            {
                if ((reporting[i] - reporting[i - 1]) > 7) yield return reporting[i] - 6;
                if (reporting[i] >= 16)
                    yield return reporting[i];
                if ((i == reporting.Length - 1) || (reporting[i + 1] - reporting[i]) > 7)
                {
                    yield return reporting[i] + 7;
                    yield return reporting[i] + 14;
                }
            }
        }

        private static IEnumerable<int> ReportingPoints()
        {
            var startWeekly = 14; // i.e. from day 28 onwards: 27-28 last daily interval
            var startQuarterly = 1099;
            var startAnnual = 3656 - 1;
            var start30Months = 7308;
            var start5Years = 10048;

            var currentDay = 0.0;
            while (currentDay < startWeekly)
            {
                yield return (int)currentDay;
                currentDay += 1;
            }

            while (currentDay < startQuarterly)
            {
                yield return (int)currentDay;
                currentDay += 7;
            }

            while (currentDay < startAnnual)
            {
                yield return (int)Math.Round(currentDay + 0.01);
                currentDay += 365.25 / 4.0;
            }

            while (currentDay < start30Months)
            {
                yield return (int)Math.Round(currentDay + 0.01);
                currentDay += 365.25;
            }

            while (currentDay < start5Years)
            {
                yield return (int)Math.Round(currentDay + 0.01);
                currentDay += 365.25 * 2.5;
            }

            yield return 10961;
            yield return 12784;
            yield return 14610;
            yield return 18263;

        }
    }
}
