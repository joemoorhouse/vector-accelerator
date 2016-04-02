using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskEngine.Framework
{
    public class SimulationRunner
    {
        public SimulationRunner(IEnumerable<SimulationSet> simulationSets, Context context)
        {
            _context = context;
            _simulationSets = simulationSets.ToList();
        }

        public void Prepare()
        {
            _currentDateTime = _context.Settings.SimulationIntervals.First().Previous;
            foreach (var set in _simulationSets)
            {
                // this provides another axis for parallelising the code 
                foreach (var model in set.Models)
                {
                    model.Prepare(_context);
                }
            }
        }

        /// <summary>
        /// Simulate over all simulation intervals (defined in Settings)
        /// </summary>
        public void Simulate()
        {
            var intervals = _context.Settings.SimulationIntervals;
            for (int i = 0; i < intervals.Length; ++i)
            {
                Step(intervals[i]);
            }
        }

        /// <summary>
        /// Step forward in time over the interval provided
        /// </summary>
        /// <param name="interval"></param>
        public void Step(TimeInterval interval)
        {
            if (_currentDateTime != interval.Previous) throw new ArgumentException("intervals are not continuous");
            foreach (var set in _simulationSets)
            {
                // this provides another axis for parallelising the code 
                foreach (var model in set.Models)
                {
                    model.StepNext();
                }
            }
            _currentDateTime = interval.Next;
        }

        /// <summary>
        /// Step forward in time over the next simulation interval (defined in Settings)
        /// </summary>
        public bool StepNext()
        {
            var intervals = _context.Settings.SimulationIntervals;
            if (_nextIntervalIndex == intervals.Length) return false;
            var interval = intervals[_nextIntervalIndex];
            if (_currentDateTime != interval.Previous) throw new ArgumentException("intervals are not continuous");
            Step(interval);
            _currentDateTime = interval.Next;
            _nextIntervalIndex++;
            return true;
        }

        Context _context;
        DateTime _currentDateTime;
        int _nextIntervalIndex = 0;
        List<SimulationSet> _simulationSets;

    }
}
