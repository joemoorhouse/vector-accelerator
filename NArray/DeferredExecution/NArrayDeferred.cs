using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NArray.Interfaces;
using NArray.DeferredExecution;

namespace NArray
{
    public class NArrayDeferred
    {
        public static IList<NArray> Evaluate(Func<NArray> function, IList<NArray> independentVariables, 
            StringBuilder expressionsOut = null,
            Aggregator aggregator = Aggregator.ElementwiseAdd, IList<NArray> existingStorage = null, 
            VectorExecutionOptions vectorOptions = null)
        {
            if (existingStorage != null && existingStorage.Count != independentVariables.Count + 1)
                throw new ArgumentException(string.Format("storage provided does not match requirement for 1 result and {0} derivatives",
                    independentVariables.Count));

            var timer = new ExecutionTimer();
            timer.Start();
            var outputs = new NArray[independentVariables.Count + 1];

            ImmediateNArrayOperationExecutor immediateExecutor = ExecutionContext.Executor as ImmediateNArrayOperationExecutor;
            if (immediateExecutor == null) throw new Exception("no immediate-mode executor found");

            var factory = immediateExecutor.Factory;
            var deferringExecutor = new DeferringNArrayOperationExecutor(independentVariables, factory);
            // note this is thread-static
            ExecutionContext.Executor = deferringExecutor;

            NArray dependentVariable;
            try
            {
                // execute function as deferred operations and obtain reference to the dependentVariable
                dependentVariable = function();
            }
            finally
            {
                ExecutionContext.Executor = immediateExecutor;
            }
            timer.MarkFunctionComplete();
            for (int i = 0; i < outputs.Length; ++i)
            {
                // if new storage is required, we create scalars in the first instance
                outputs[i] = (existingStorage == null) ? factory.NewScalarNArray(0) as NArray : existingStorage[i] as NArray;
            }
            
            BlockExpressionEvaluator.Evaluate(vectorOptions, immediateExecutor, deferringExecutor.Builder, outputs,
                dependentVariable, independentVariables, timer, expressionsOut, aggregator);

            //Console.WriteLine(timer.Report());
            return outputs;
        }
    }

    public partial class NArray
    {
        public static IList<NArray> Evaluate(Func<NArray> function, params NArray[] independentVariables)
        {
            return NArrayDeferred.Evaluate(function, independentVariables);
        }

        public static IList<NArray> Evaluate(Func<NArray> function, StringBuilder expressionsOut, params NArray[] independentVariables)
        {
            return NArrayDeferred.Evaluate(function, independentVariables, expressionsOut);
        }


        //    public static IList<NArray> Evaluate(Func<NArray> function, IList<NArray> independentVariables,
        //Aggregator aggregator, IList<NArray> existingStorage)
        //    {
        //        return VectorAccelerator.DeferredExecution.DeferredExecutionContext.Evaluate(function, independentVariables, null, aggregator, existingStorage);
        //    }

        //    public static NArray Evaluate(Func<NArray> function)
        //    {
        //        return VectorAccelerator.DeferredExecution.DeferredExecutionContext.Evaluate(function, new List<NArray>()).First();
        //    }

        //    public static void Evaluate(NArray result, Func<NArray> function)
        //    {
        //        VectorAccelerator.DeferredExecution.DeferredExecutionContext.Evaluate(function, new List<NArray>(), null, Aggregator.ElementwiseAdd, new List<NArray> { result }).First();
        //    }

        //    public static IList<NArray> Evaluate(VectorExecutionOptions vectorOptions, Func<NArray> function)
        //    {
        //        return VectorAccelerator.DeferredExecution.DeferredExecutionContext.Evaluate(function, new List<NArray>(), null, Aggregator.ElementwiseAdd, null, vectorOptions);
        //    }

        //    public static IList<NArray> Evaluate(Func<NArray> function, params NArray[] independentVariables)
        //    {
        //        return VectorAccelerator.DeferredExecution.DeferredExecutionContext.Evaluate(function, independentVariables);
        //    }

        //    public static IList<NArray> Evaluate(Func<NArray> function, StringBuilder expressionsOut, params NArray[] independentVariables)
        //    {
        //        return VectorAccelerator.DeferredExecution.DeferredExecutionContext.Evaluate(function, independentVariables, expressionsOut);
        //    }
    }
}
