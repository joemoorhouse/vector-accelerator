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

        public Context(StorageLocation location)
        {
            Factory = new NArrayFactory(location);
        }
    }

    public class SimulationSettings
    {
        public int SimulationCount { get; set; }

        public SimulationSettings()
        {
            SimulationCount = 5000;
        }
    }
}
