using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator;

namespace RiskEngine.Framework
{
    public interface IPath
    {
        NArray this[int timeIndex] { get; }
    }
    
    public interface IPath<T> : IPath
    {
        T Process { get; }
    }
    
    public class PathModel<T> : StoringModel<T>, IPath where T : Process, new()
    {
        List<NArray> _storage = new List<NArray>();

        public NArray this[int timeIndex]
        {
            get { return _storage[timeIndex]; }
        }

        public PathModel() : base() { }

        public override void Initialise(SimulationGraph graph)
        {
            base.Initialise(graph);
            _singleFactorProcess.Initialise(graph);
        }

        public override void Prepare(Context context)
        {
            _storage.Add(_singleFactorProcess.Prepare(context));
        }

        public override void StepNext(TimeInterval interval)
        {
            _storage.Add(
                _singleFactorProcess.Step(interval, _storage[_timeIndex])
                );
            _timeIndex++;
        }
    }
}
