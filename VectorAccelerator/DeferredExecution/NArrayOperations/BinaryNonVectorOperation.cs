using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator.DeferredExecution.NArrayOperations
{
    public class BinaryNonVectorOperation<T> : BinaryOperation<T>
    {
        public BinaryNonVectorOperation(NArray<T> operand1, NArray<T> operand2, NArray<T> result,
            Action<NArray<T>, NArray<T>, NArray<T>> operation)
            : base(operand1, operand2, result, operation) { }
          
        public override bool IsVectorOperation
        {
            get { return false; }
        }
    }
}
