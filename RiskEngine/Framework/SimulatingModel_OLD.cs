using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator;

namespace RiskEngine.Framework
{
    public abstract class SimulatingModel_OLD : Model, IFactor
    {
        public NArray Value { get { return _value; } }

        protected NArray _value;

        internal abstract void Prepare(Context context);

        /// <summary>
        /// Simulate using the interval provided. 
        /// </summary>
        /// <param name="timeStep">Interval from current to next time point.</param>
        internal abstract void Step(TimeInterval timeStep);
    }
}
