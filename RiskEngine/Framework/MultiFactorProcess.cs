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
    public abstract class MultiFactorProcess : Model
    {
        public List<IdentifiedNArray> Factors { get { return _factors; } }
        
        public IdentifiedNArray AddFactor(string identifier)
        {
            var factor = new IdentifiedNArray(identifier);
            _factors.Add(factor);
            return factor;
        }

        public NArray Value { get; protected set; }

        internal abstract void Prepare(Context context);

        internal override void StepNext()
        {
            var timeStep = _timeIntervals[_currentTimeIndex];
            Step(timeStep);
            _currentTimeIndex++;
        }

        protected abstract void Step(TimeInterval timeStep);

        protected List<IdentifiedNArray> _factors = new List<IdentifiedNArray>();
        protected NArrayFactory _factory;
    }
}
