using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator;

namespace RiskEngine.Framework
{
    public interface IPath<T>
    {
        NArray this[int timeIndex] { get; }

        T Process { get; }
    }
    
    public class PathModel<T> : StoringModel<T> where T : Process, new()
    {
        List<NArray> _storage = new List<NArray>();

        public NArray this[int timeIndex]
        {
            get { return _storage[timeIndex]; }
        }

        public T Process { get { return _singleFactorProcess; } }

        public PathModel() : base() { }

        public override void Initialise(Simulation simulation)
        {
            _singleFactorProcess.Initialise(simulation);
        }

        public override void Prepare(Context context)
        {
            _storage[0] = _singleFactorProcess.Prepare(context);
        }

        public override void StepNext()
        {
            _storage.Add(
                _singleFactorProcess.Step(_timeIntervals[_timeIndex], _storage[_timeIndex])
                );
            _timeIndex++;
        }
    }
}
