using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator.DeferredExecution
{
    public class DeferredExecutionContext : IDisposable
    {
        DeferredPrimitivesExecutor _executor;
        IExecutor _previousExecutor;
        VectorExecutionOptions _options;

        public DeferredExecutionContext(VectorExecutionOptions options)
        {
            //_executor = new ExpressionBuildingExecutor();
            _executor = new DeferredPrimitivesExecutor();
            _previousExecutor = ExecutionContext.Executor;
            _options = options;
            ExecutionContext.Executor = _executor;
        }

        public void Dispose()
        {
            _executor.Execute(_options);
            ExecutionContext.Executor = _previousExecutor;
        }
    }
}
