using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator;
using RiskEngine.Models;

namespace RiskEngine.Framework
{
    /// <summary>
    /// Factor that holds all factors values (per scenario) for each simulation time point.
    /// </summary>
    public class PathFactor : Factor
    {
        public NArray this[int timeIndex]
        {
            get { return Path[timeIndex];  }
        }
        
        /// <summary>
        /// NArray of scenario values for each simulation time point.
        /// </summary>
        public readonly List<NArray> Path;
    }

    /// <summary>
    /// Factor that holds all factors values (per scenario) for each simulation time point.
    /// </summary>
    /// <typeparam name="T">Interface implemented by the Process.</typeparam>
    public class PathFactor<T> : Factor<IPath<T>> where T : IProcess
    {
        public NArray this[int timeIndex]
        {
            get { return Model[timeIndex]; }
        }
    }
}
