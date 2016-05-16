using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskEngine.Framework
{
    /// <summary>
    /// This stores NArrays that evolve over time according to a Process.
    /// </summary>
    public abstract class StoringModel<T> : EvolvingModel where T : Process, new()
    {
        public override string Identifier
        {
            internal set
            {
                base.Identifier = _singleFactorProcess.Identifier = value;
            }
        }
        
        protected T _singleFactorProcess;
        protected int _timeIndex;

        public StoringModel()
        {
            _singleFactorProcess = new T();
        }
    }
}
