using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator;
using RiskEngine.Data;

namespace RiskEngine.Framework
{
    /// <summary>
    /// A MultiFactorModel simulates and stores a number of factors (each factor being vector NArray). 
    /// A call to Step updates the stored simulations with the new values.
    /// </summary>
    public abstract class MultiFactorModel : EvolvingModel
    {
        public List<NArray> Factors { get { return _factors; } }
        
        public NArray AddFactor(string identifier, SimulationGraph graph)
        {
            var context = graph.Context;
            var factor = context.Factory.CreateNArray(context.Settings.SimulationCount, 1);
            _factors.Add(factor);
            _factorIdentifiers.Add(identifier);
            return factor;
        }

        protected List<NArray> _factors = new List<NArray>();
        protected List<string> _factorIdentifiers = new List<string>();
        protected NArrayFactory _factory;
    }
}
