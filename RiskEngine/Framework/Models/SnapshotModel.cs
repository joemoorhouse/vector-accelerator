using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator;

namespace RiskEngine.Framework
{
    public class SnapshotModel<T> : StoringModel<T> where T : Process, new()
    {
        NArray _value;

        public NArray Value { get { return _value; } }

        public SnapshotModel() : base() { }

        public override void Initialise(Simulation simulation)
        {
            _singleFactorProcess.Initialise(simulation);
        }

        public override void Prepare(Context context)
        {
            _value = _singleFactorProcess.Prepare(context);
        }

        public override void StepNext()
        {
            _value.Assign(
                _singleFactorProcess.Step(_timeIntervals[_timeIndex], _value)
                );
        }
    }
}
