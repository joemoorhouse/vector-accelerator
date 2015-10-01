using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator.DeferredExecution
{
    /// <summary>
    /// Operation with two vector operands that returns a new vector.
    /// </summary>
    public class BinaryVectorOperation : NArrayOperation
    {
        public readonly NArray Operand1;
        public readonly NArray Operand2;
        public readonly Action<NArray, NArray, NArray> Operation;

        public BinaryVectorOperation(NArray operand1, NArray operand2, NArray result,
            Action<NArray, NArray, NArray> operation)
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

        public override NArrayOperation Clone(Func<NArray, NArray> transform)
        {
            return new BinaryVectorOperation(transform(Operand1), transform(Operand2), transform(Result), Operation);
        }

        public override IList<NArray> Operands()
        {
            return new List<NArray> { Operand1, Operand2 };
        }
    }
}
