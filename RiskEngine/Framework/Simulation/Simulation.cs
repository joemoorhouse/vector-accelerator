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
        
        /// <summary>
        /// Requests a Factor by Factor type and identifier, i.e. a risk factor or random process.
        /// Use this when the actal Model to be used should be configurable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public T RegisterFactor<T>(string identifier) where T : class
        {
            var factor = _graph.GetFactor<T>(identifier, this);
            return factor;
        }

        /// <summary>
        /// Requests a Model by Model type and identifier. Use this to bypass the Factor abstraction (e.g. when the
        /// specified Model type is always required).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public T RegisterModel<T>(string identifier) where T : class
        {
            var model = _graph.GetModel<T>(identifier, this);
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
