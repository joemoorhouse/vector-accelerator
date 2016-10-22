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
    public abstract class StoringModel<T> : Model, IEvolvingModel where T : Process, new()
    {
        public override string Identifier
        {
            internal set
            {
                base.Identifier = value;
                _singleFactorProcess.Identifier = value;
            }
        }

        public abstract void StepNext(TimeInterval interval);

        public T Process { get { return _singleFactorProcess; } }

        protected T _singleFactorProcess;
        protected int _timeIndex;

        public StoringModel()
        {
            _singleFactorProcess = new T();
        }
    }
}
