using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator;

namespace RiskEngine.Framework
{
    /// <summary>
    /// Stores the simulation graph for a particular simulation as well as data required to populate graph.
    /// </summary>
    public class Simulation
    {
        /// <summary>
        /// Provides data required for the Simulation
        /// </summary>
        public readonly Context Context;

        public T DefaultModel<T>(string identifier) where T : class
        {
            var model = _graph.DefaultModel<T>(identifier, this);
            return model;
        }

        public T ModelOfType<T>(string identifier) where T : class
        {
            var model = _graph.ModelOfType<T>(identifier, this);
            return model;
        }

        public Simulation(StorageLocation location, DateTime closeOfBusinessDate)
        {
            Context = new Context(location, closeOfBusinessDate);
        }

        public SimulationRunner CreateSimulationRunner()
        {
            return _graph.ToSimulationRunner(Context);
        }

        SimulationGraph _graph = new SimulationGraph();
    }
}
