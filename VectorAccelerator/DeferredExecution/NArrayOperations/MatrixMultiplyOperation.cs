using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator.DeferredExecution
{
    public class MatrixMultiplyOperation : NArrayOperation
    {
        public readonly NArray Operand1;
        public readonly NArray Operand2;
        public readonly Action<NArray, NArray, NArray> Operation;

        public MatrixMultiplyOperation(NArray operand1, NArray operand2, NArray result,
            Action<NArray, NArray, NArray> operation)
        {
            Operand1 = operand1;
            Operand2 = operand2;
            Result = result;
            Operation = operation;
        }
        
        public override bool IsVectorOperation
        {
            get { return false; }
        }
    }
}
