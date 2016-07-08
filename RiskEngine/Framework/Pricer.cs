using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator;

namespace RiskEngine.Framework
{
    public interface IPricer
    {
        void PrePrice();
        
        void Price(int timeIndex, out NArray pv);

        void Register(SimulationGraph graph);
    }
    
    public class Pricer<T> where T : Deal, new()
    {
        protected T _deal = new T();
        //protected TimePoint[] _timePoints;
        protected DateTime[] _timePoints;

        public T Deal { get { return _deal; } }

        public virtual void Register(SimulationGraph graph)
        {
            var timePoints = graph.Context.Settings.SimulationTimePoints;
            _timePoints = timePoints.Select(t => t.DateTime).ToArray();
        }
    }
}
