using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator;

namespace RiskEngine.Framework
{
    public interface IValue
    {
        NArray Value { get; }
    }

    public interface IValue<T> : IValue
    {
        T Process { get; }
    }
    
    public class SnapshotModel<T> : StoringModel<T>, IValue where T : Process, new()
    {
        NArray _value;

        public NArray Value { get { return _value; } }

        public SnapshotModel() : base() { }

        public override void Initialise(SimulationGraph graph)
        {
            base.Initialise(graph);
            _singleFactorProcess.Initialise(graph);
        }

        public override void Prepare(Context context)
        {
            _value = _singleFactorProcess.Prepare(context);
        }

        public override void StepNext()
        {
            _value = _singleFactorProcess.Step(_timeIntervals[_timeIndex], _value);
        }
    }
}
