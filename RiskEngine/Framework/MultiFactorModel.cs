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
    public abstract class MultiFactorModel
    {
        public List<Factor> Factors { get { return _factors; } }
        
        public IFactor AddFactor(string identifier)
        {
            var factor = new Factor(this, identifier);
            _factors.Add(factor);
            return factor;
        }

        public virtual void Prepare(Context context)
        { 
            // no preparation in base
        }

        public abstract void Step(TimeInterval timeStep);

        protected List<Factor> _factors = new List<Factor>();
        protected NArrayFactory _factory;

        public class Factor : IFactor
        {
            public NArray Value { get { return _value; } set { _value = value; } }

            public readonly string Identifier;

            public Factor(MultiFactorModel model, string identifier)
            {
                _model = model; Identifier = identifier;
            }

            private MultiFactorModel _model;
            private NArray _value;
        }
    }
}
