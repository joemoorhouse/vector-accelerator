using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator;

namespace RiskEngine.Framework
{
    /// <summary>
    /// Factor that holds a snapshot of a single value (per scenario) for the current simulation time point.
    /// </summary>
    public class SnapshotFactor : Factor
    {
        /// <summary>
        /// NArray of scenario values for current simulation time point.
        /// </summary>
        public readonly NArray Value;

        public SnapshotFactor()
        {
            Value = null; // new NArray(null); // create null storage NArray
        }
    }

    public class SnapshotFactor<T> : Factor<T> where T : class
    {
        /// <summary>
        /// NArray of scenario values for current simulation time point.
        /// </summary>
        public readonly NArray Value;

        public SnapshotFactor()
        {
            Value = new NArray(null); // create null storage NArray
        }
    }
}
