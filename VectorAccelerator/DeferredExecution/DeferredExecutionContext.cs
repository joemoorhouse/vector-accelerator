using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator.DeferredExecution
{
    public class DeferredExecutionContext : IDisposable
    {
        DeferredExecutionExecutor _executor;
        IExecutor _previousExecutor;

        public DeferredExecutionContext()
        {
            _executor = new DeferredExecutionExecutor();
            _previousExecutor = ExecutionContext.Executor;
            ExecutionContext.Executor = _executor;
        }

        public void Dispose()
        {
            _executor.EndDeferredExecution();
            ExecutionContext.Executor = _previousExecutor;
        }
    }
}
