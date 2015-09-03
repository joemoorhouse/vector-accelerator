using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using VectorAccelerator.LinearAlgebraProviders;

namespace VectorAccelerator.DeferredExecution
{
    public class VectorOperation
    {
        public NArray Result;
    }

    /// <summary>
    /// 
    /// </summary>
    public class BinaryVectorOperation : VectorOperation
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
    }

    public class UnaryVectorOperation : VectorOperation
    {
        public readonly NArray Operand;
        public readonly Action<NArray, NArray> Operation;

        public UnaryVectorOperation(NArray operand, NArray result, Action<NArray, NArray> operation)
        {
            Operand = operand;
            Result = result;
            Operation = operation;
        }
    }

    public class AssignOperation : VectorOperation
    {
        public readonly NArray Left;
        public readonly NArray Right;

        public AssignOperation(NArray left, NArray right)
        {
            Left = left;
            Right = right;
        }
    }
}
