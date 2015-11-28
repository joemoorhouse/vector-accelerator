using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator;

namespace RiskEngine.Framework
{
    public interface IModel : IFactor
    {
        void Step(double timeStep);
    }
    
    /// <summary>
    /// A Model simulates and stores a single factor (i.e. vector NArray). 
    /// A call to Step updates the stored simulations with the new values.
    /// </summary>
    public abstract class Model : IFactor
    {
        public NArray Value { get { return _value; } }

        /// <summary>
        /// Simulate using the interval provided. 
        /// </summary>
        /// <param name="timeStep">Interval from current to next time point.</param>
        public abstract void Step(TimeInterval timeStep);

        protected NArray _value;
    }


}
