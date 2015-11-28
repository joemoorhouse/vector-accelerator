using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator
{
    public class NArrayBool : NArray<bool>
    {
        public NArrayBool(StorageLocation location, int length) : base(location, length) { }

        public static NArrayBool operator &(NArrayBool operand1, NArrayBool operand2)
        {
            return ExecutionContext.Executor.LogicalOperation(operand1, operand2, LogicalBinaryElementWiseOperation.And);
        }

        public static bool operator true(NArrayBool operand)
        {
            return true;
        }

        public static bool operator false(NArrayBool operand)
        {
            return false;
        }

        public static NArrayBool operator |(NArrayBool operand1, NArrayBool operand2)
        {
            return ExecutionContext.Executor.LogicalOperation(operand1, operand2, LogicalBinaryElementWiseOperation.Or);
        }
    }
}
