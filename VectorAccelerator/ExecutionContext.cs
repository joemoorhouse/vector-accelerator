using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator.DeferredExecution;

namespace VectorAccelerator
{
    internal sealed class ExecutionContext
    {
        #region Singleton

        static readonly ExecutionContext _instance = new ExecutionContext();

        static ExecutionContext() { }

        private static ExecutionContext Instance
        {
            get
            {
                return _instance;
            }
        }

        #endregion

        IExecutor _executor;

        ExecutionContext()
        {
            _executor = new ImmediateExecutor();
        }

        public static IExecutor Executor
        {
            get { return Instance._executor; }
            set { Instance._executor = value; }
        }

        //public static void ExecuteDeferred(Action function)
        //{
        //    Instance._executionMode = ExecutionMode.Deferred;
        //    Instance._deferredExecutionExecutor.StartDeferredExecution();
        //    function();
        //    Instance._deferredExecutionExecutor.EndDeferredExecution();
        //}
    }

}
