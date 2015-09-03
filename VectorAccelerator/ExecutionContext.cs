using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator.DeferredExecution;
using VectorAccelerator.NArrayStorage;
using System.Threading;

namespace VectorAccelerator
{
    /// <summary>
    /// A singleton class such that every thread will see a different instance. This allows
    /// vector operations to be executed separately on different threads if required.
    /// </summary>
    internal sealed class ExecutionContext
    {
        #region Singleton

        [ThreadStatic]
        static ExecutionContext _instance;

        static ExecutionContext() { }

        private static ExecutionContext Instance
        {
            get
            {
                if (_instance == null) _instance = new ExecutionContext();
                return _instance;
            }
        }

        #endregion

        IExecutor _executor;
        ArrayPool _arrayPool = new ArrayPool();
        string _threadName;

        ExecutionContext()
        {
            _executor = new ImmediateExecutor();
            _threadName = Thread.CurrentThread.Name;
        }

        public static IExecutor Executor
        {
            get { return Instance._executor; }
            set { Instance._executor = value; }
        }

        public static ArrayPool ArrayPool
        {
            get { return Instance._arrayPool; }
        }
    }

}
