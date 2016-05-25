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
            var simulationDateTimes = SimulationDateTimes(closeOfBusinessDate).ToList();
            
            SimulationIntervals = simulationDateTimes.Skip(1)
                .Zip(simulationDateTimes, (n, p) => new TimeInterval(p, n))
                .ToArray();

            SimulationTimePoints = simulationDateTimes
                .Select(p => new TimePoint(p, (p - simulationDateTimes[0]).Days / 365.25))
                .ToArray();
        }

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
    }
}
