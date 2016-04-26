using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator.DeferredExecution
{
    public class DeferredExecutionContext : IDisposable
    {
        DeferringExecutor _executor;
        IExecutor _previousExecutor;
        VectorExecutionOptions _options;

        public DeferredExecutionContext(VectorExecutionOptions options)
        {
            //_executor = new ExpressionBuildingExecutor();
            _executor = new DeferringExecutor();
            _previousExecutor = ExecutionContext.Executor;
            _options = options;
            ExecutionContext.Executor = _executor;
        }

        public static IList<NArray> Evaluate(Func<NArray> function, params NArray[] independentVariables)
        {
            NArray[] outputs = null;
            try
            {
                foreach (var variable in independentVariables)
                {
                    variable.IsIndependentVariable = true; // revisit?
                }
                using (var context = new DeferredExecutionContext(new VectorExecutionOptions()))
                {
                    // execute function as deferred operations and obtain reference to the dependentVariable
                    var dependentVariable = function();
                    context._executor.Evaluate(context._options, out outputs, dependentVariable, independentVariables);
                }
            }
            finally
            {
                foreach (var variable in independentVariables)
                {
                    variable.IsIndependentVariable = false;
                }
            }
            return outputs;
        }

        public void Dispose()
        {
            _executor.Execute(_options);
            ExecutionContext.Executor = _previousExecutor;
        }
    }
}
