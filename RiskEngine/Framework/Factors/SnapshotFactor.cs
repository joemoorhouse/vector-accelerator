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
    public class SnapshotFactor : Factor<IValue>
    {
        public NArray Value { get { return Model.Value; } }
    }

    public class SnapshotFactor<T> : Factor<IValue<T>> where T : class
    {
        public NArray Value { get { return Model.Value; } }
    }
}
