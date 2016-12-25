using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NArray
{
    public class NMath
    {
        public static NArray Exp(NArray operand)
        {
            return ExecutionContext.Executor.UnaryElementWiseOperation(operand, UnaryElementWiseOperations.Exp) as NArray;
        }

        public static NArray Log(NArray operand)
        {
            return ExecutionContext.Executor.UnaryElementWiseOperation(operand, UnaryElementWiseOperations.Log) as NArray;
        }

        public static NArray CumulativeNormal(NArray operand)
        {
            return ExecutionContext.Executor.UnaryElementWiseOperation(operand, UnaryElementWiseOperations.CumulativeNormal) as NArray;
        }
    }
}
