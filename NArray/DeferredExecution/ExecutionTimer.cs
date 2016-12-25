using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace NArray.DeferredExecution
{
    public class ExecutionTimer
    {
        private Stopwatch _watch;

        private long _functionComplete; // call the function itself; evaluate any scalars; create expression block
        private long _evaluateSetUpComplete; // light set-up tasks
        private long _differentiationComplete; // perform differentation; extend expression block
        private long _storageSetupComplete; // create any storgage necessary for the results
        private long _executionTemporaryStorageAllocationComplete; // obtain storage for temporary results
        private long _executionComplete; // execute the vector instructions

        public void Start()
        {
            _watch = new Stopwatch();
            _watch.Start();
        }

        public void MarkFunctionComplete()
        {
            _functionComplete = _watch.ElapsedTicks;
        }

        public void MarkEvaluateSetupComplete()
        {
            _evaluateSetUpComplete = _watch.ElapsedTicks;
        }

        public void MarkDifferentationComplete()
        {
            _differentiationComplete = _watch.ElapsedTicks;
        }

        public void MarkStorageSetupComplete()
        {
            _storageSetupComplete = _watch.ElapsedTicks;
        }

        public void MarkExecutionTemporaryStorageAllocationComplete()
        {
            _executionTemporaryStorageAllocationComplete = _watch.ElapsedTicks;
        }

        public void MarkExecuteComplete()
        {
            _executionComplete = _watch.ElapsedTicks;
        }

        public string Report()
        {
            var builder = new StringBuilder();
            //var usPerTick = 1000000.0 / (double)TimeSpan.TicksPerSecond;
            var usPerTick = 1e6 / Stopwatch.Frequency;
            builder.AppendLine(string.Format("Total elapsed: {0} us", _executionComplete * usPerTick));
            builder.AppendLine(string.Format("Function call: {0} us", _functionComplete * usPerTick));
            builder.AppendLine(string.Format("Evaluation set-up: {0} us", (_evaluateSetUpComplete - _functionComplete) * usPerTick));
            builder.AppendLine(string.Format("Differentiation: {0} us", (_differentiationComplete - _evaluateSetUpComplete) * usPerTick));
            builder.AppendLine(string.Format("Storage set-up: {0} us", (_storageSetupComplete - _differentiationComplete) * usPerTick));
            builder.AppendLine(string.Format("Temporary storage set-up: {0} us", (_executionTemporaryStorageAllocationComplete - _storageSetupComplete) * usPerTick));
            builder.AppendLine(string.Format("Execute: {0} us", (_executionComplete - _executionTemporaryStorageAllocationComplete) * usPerTick));
            return builder.ToString();
        }
    }
}
