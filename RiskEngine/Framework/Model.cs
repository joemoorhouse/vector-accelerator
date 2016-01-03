using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskEngine.Framework
{    
    /// <summary>
    /// A Model simulates and stores a single factor (i.e. vector NArray). 
    /// A call to Step updates the stored simulations with the new values.
    /// </summary>
    public class Model 
    {
        public string Identifier { get; private set; }

        public Context Context { get; private set; }

        public DateTime CurrentTimePoint { get { return _timeIntervals[_currentTimeIndex].Previous; } }

        protected Model() { }

        public static T Create<T>(string identifier, Simulation simulation) where T : Model, new()
        {
            var model = new T();
            model.Identifier = identifier;
            model.Context = simulation == null ? null : simulation.Context;
            return model;
        }

        public virtual void Initialise(Simulation simulation) { }

        public override string ToString()
        {
            return this.GetType().Name + ":" + Identifier;
        }

        internal virtual void StepNext()
        {
            _currentTimeIndex++;
        }

        protected int _currentTimeIndex;
        protected TimeInterval[] _timeIntervals;
    }

    public class DummyModel : Model
    {
    }
}
