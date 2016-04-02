using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskEngine.Framework
{    
    /// <summary>
    /// A Model participates in the simulation graph.
    /// </summary>
    public class Model 
    {
        public string Identifier { get; internal set; }

        protected Model() { }

        public static T Create<T>(string identifier, Simulation simulation) where T : Model, new()
        {
            var model = new T();
            model.Identifier = identifier;
            return model;
        }

        public virtual void Initialise(Simulation simulation) { }

        public override string ToString()
        {
            return this.GetType().Name + ":" + Identifier;
        }

        protected TimePoint[] _timePoints;
        protected TimeInterval[] _timeIntervals;
    }

    public class DummyModel : Model
    {
    }
}
