using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator.DeferredExecution
{
    public class VectorExecutionOptions
    {
        public bool MultipleThreads = false;
    }

    public class DeferredExecutionContext 
    {
        DeferringExecutor _executor;
        IExecutor _previousExecutor;
        VectorExecutionOptions _options;

        private DeferredExecutionContext(VectorExecutionOptions options, IList<NArray> independentVarables)
        {
            _executor = new DeferringExecutor(independentVarables);
            _previousExecutor = ExecutionContext.Executor;
            _options = options;
            ExecutionContext.Executor = _executor;
        }

        public static IList<NArray> Evaluate(Func<NArray> function, IList<NArray> independentVariables, StringBuilder expressionsOut = null,
            Aggregator aggregator = Aggregator.ElementwiseAdd, IList<NArray> existingStorage = null, VectorExecutionOptions vectorOptions = null)
        {
            if (existingStorage != null && existingStorage.Count != independentVariables.Count + 1)
                throw new ArgumentException(string.Format("storage provided does not match requirement for 1 result and {0} derivatives",
                    independentVariables.Count));

            var timer = new ExecutionTimer();
            timer.Start();
            NArray[] outputs = new NArray[independentVariables.Count + 1];

            var context = new DeferredExecutionContext(new VectorExecutionOptions(), independentVariables);
            NArray dependentVariable;
            try
            {
                // execute function as deferred operations and obtain reference to the dependentVariable
                dependentVariable = function();
            }
            finally
            {
                context.Finish();
            }
            timer.MarkFunctionComplete();
            for (int i = 0; i < outputs.Length; ++i)
            {
                // if new storage is required, we create scalars in the first instance
                outputs[i] = (existingStorage == null) ? NArray.CreateScalar(0) : existingStorage[i];
            }
            context._executor.Evaluate(context._options, outputs, dependentVariable, independentVariables, 
                timer, expressionsOut, aggregator);

            //Console.WriteLine(timer.Report());
            return outputs;
        }

        private void Finish()
        {
            ExecutionContext.Executor = _previousExecutor;
        }
    }
}
