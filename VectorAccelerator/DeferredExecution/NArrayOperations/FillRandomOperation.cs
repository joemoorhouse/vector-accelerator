using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator.DeferredExecution.VectorOperations
{
    class FillRandomOperation<T> : NArrayOperation<T>
    {
        public override bool IsVectorOperation
        {
            get { return true; }
        }
    }
}
