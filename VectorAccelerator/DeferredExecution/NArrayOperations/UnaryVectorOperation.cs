using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator.DeferredExecution
{
    /// <summary>
    /// Operation with a single vector operand that returns a new vector.
    /// </summary>
    public class UnaryVectorOperation<T> : NArrayOperation<T>
    {
        public readonly NArray<T> Operand;
        public readonly UnaryElementWiseOperation OperationType;
        public readonly Action<NArray<T>, NArray<T>> Operation;

        public UnaryVectorOperation(NArray<T> operand, NArray<T> result, Action<NArray<T>, NArray<T>> operation)
        {
            Operand = operand;
            Result = result;
            Operation = operation;
        }

        public override bool IsVectorOperation
        {
            get { return true; }
        }

        public override string ToString()
        {
            return string.Join(" ", Result.ToString(), "=", Operation.Method.Name, "(" + Operand.ToString() + ")");
        }

        private string OperationString()
        {
            if (Operation.Method.Name == "Multiply")
                return "*";
            else if (Operation.Method.Name == "Add")
                return "+";
            else if (Operation.Method.Name == "Subtract")
                return "-";
            else if (Operation.Method.Name == "Divide")
                return "/";
            else return "?";
        }

        public override NArrayOperation<T> Clone(Func<NArray<T>, NArray<T>> transform)
        {
            return new UnaryVectorOperation<T>(transform(Operand), transform(Result), Operation);
        }

        public override IList<NArray<T>> Operands()
        {
            return new List<NArray<T>> { Operand };
        }
    }
}
