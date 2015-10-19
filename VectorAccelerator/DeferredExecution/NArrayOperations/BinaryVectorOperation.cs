using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator.DeferredExecution
{
    /// <summary>
    /// Operation with two vector operands that returns a new vector.
    /// </summary>
    public class BinaryVectorOperation<T> : NArrayOperation<T>
    {
        public readonly NArray<T> Operand1;
        public readonly NArray<T> Operand2;
        public readonly Action<NArray<T>, NArray<T>, NArray<T>> Operation;

        public BinaryVectorOperation(NArray<T> operand1, NArray<T> operand2, NArray<T> result,
            Action<NArray<T>, NArray<T>, NArray<T>> operation)
        {
            Operand1 = operand1;
            Operand2 = operand2;
            Result = result;
            Operation = operation;
        }

        public override bool IsVectorOperation
        {
            get { return true; }
        }

        public override string ToString()
        {
            return string.Join(" ", Result.ToString(), "=", Operand1.ToString(), OperationString(), Operand2.ToString());
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
            return new BinaryVectorOperation<T>(transform(Operand1), transform(Operand2), transform(Result), Operation);
        }

        public override IList<NArray<T>> Operands()
        {
            return new List<NArray<T>> { Operand1, Operand2 };
        }
    }
}
