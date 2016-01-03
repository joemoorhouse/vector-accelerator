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
        public int SimulationCount { get; private set; }

        public TimeInterval[] SimulationIntervals { get; private set; }

        public SimulationSettings(DateTime closeOfBusinessDate)
        {
            SimulationCount = 5000;
            var simulationDateTimes = SimulationDateTimes(closeOfBusinessDate);
            SimulationIntervals = simulationDateTimes.Skip(1)
                .Zip(simulationDateTimes, (n, p) => new TimeInterval(p, n))
                .ToArray();     
        }

        private IEnumerable<DateTime> SimulationDateTimes(DateTime start)
        {
            var startDateTime = start;
            var startWeekly = startDateTime.AddMonths(1);
            var startMonthly = startDateTime.AddYears(3);
            var startAnnual = startDateTime.AddYears(2);
            var start5Yearly = startDateTime.AddYears(5);
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

            currentDateTime = startAnnual;
            while (currentDateTime < startAnnual)
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
