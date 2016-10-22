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
            var reportingDateTimes = ReportingDateTimes(closeOfBusinessDate).ToList();

            var simulationDateTimes = reportingDateTimes; //IncludeMarginCallAndCloseoutPoints(reportingDateTimes).ToArray();

            var check = simulationDateTimes.Select(d => (d - closeOfBusinessDate).TotalDays).ToArray();

            var check2 = reportingDateTimes.Select(d => (d - closeOfBusinessDate).TotalDays).ToArray();

            SimulationIntervals = simulationDateTimes.Skip(1)
                .Zip(
                simulationDateTimes.Select((d, i) => new { Index = i, Date = d }), 
                (n, p) => new TimeInterval(p.Date, n, p.Index))
                .ToArray();

            SimulationTimePoints = simulationDateTimes
                .Select(p => new TimePoint(p, (p - simulationDateTimes[0]).TotalDays / 365.25))
                .ToArray();
        }

        /// <summary>
        /// One choice of points
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        private IEnumerable<DateTime> ReportingDateTimes(DateTime start)
        {
            var startDateTime = start;
            var startWeekly = startDateTime.AddDays(30);
            var startMonthly = startDateTime.AddMonths(6);
            var startQuarterly = startDateTime.AddYears(2);
            var startAnnual = startDateTime.AddYears(15);
            var start5Yearly = startDateTime.AddYears(25);
            var end = startDateTime.AddYears(60);

            return GetSchedule(startDateTime, startWeekly, (c) => c.AddDays(1))
                .Concat(GetSchedule(startWeekly, startMonthly, (c) => c.AddDays(7)))
                .Concat(GetSchedule(startMonthly, startQuarterly, (c) => c.AddMonths(1)))
                .Concat(GetSchedule(startQuarterly, startAnnual, (c) => c.AddMonths(3)))
                .Concat(GetSchedule(startAnnual, start5Yearly, (c) => c.AddYears(1)))
                .Concat(GetSchedule(start5Yearly, end, (c) => c.AddYears(5)))
                .Distinct();
        }

        private IEnumerable<DateTime> GetSchedule(DateTime start, DateTime stop, Func<DateTime, DateTime> increment)
        {
            var current = start;
            while (current <= stop)
            {
                yield return current;
                current = increment(current);
            }
        }

        private static IEnumerable<DateTime> IncludeMarginCallAndCloseoutPoints(IEnumerable<DateTime> reportingSet)
        {
            var first = reportingSet.First();

            return reportingSet
                .Concat(reportingSet.Select(d => d.AddDays(-6))) // do not add if within 1 day?
                .Concat(reportingSet.Select(d => d.AddDays(7)))
                .Concat(reportingSet.Select(d => d.AddDays(14)))
                .Where(d => d > first)
                .Distinct()
                .OrderBy(d => d);
        }
    }
}
