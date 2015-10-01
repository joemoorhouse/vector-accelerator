using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator.DeferredExecution
{
    /// <summary>
    /// Operation with a single vector operand that returns a new vector.
    /// </summary>
    public class UnaryVectorOperation : NArrayOperation
    {
        public readonly NArray Operand;
        public readonly Action<NArray, NArray> Operation;

        public UnaryVectorOperation(NArray operand, NArray result, Action<NArray, NArray> operation)
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

        public override NArrayOperation Clone(Func<NArray, NArray> transform)
        {
            return new UnaryVectorOperation(transform(Operand), transform(Result), Operation);
        }

        public override IList<NArray> Operands()
        {
            return new List<NArray> { Operand };
        }
    }
}
