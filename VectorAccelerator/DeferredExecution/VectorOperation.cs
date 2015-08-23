using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace VectorAccelerator.DeferredExecution
{
    public class VectorOperation
    {
        protected List<Expression> _expressions = new List<Expression>();
    }

    /// <summary>
    /// 
    /// </summary>
    public class BinaryVectorOperation : VectorOperation
    {
        public readonly NAray Operand1;
        public readonly NAray Operand2;
        public readonly NAray Result;
        public readonly Func<Expression, Expression, BinaryExpression> Operation; 

        public BinaryVectorOperation(NAray operand1, NAray operand2, NAray result, Func<Expression, Expression, BinaryExpression> operation)
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
            if (Operation == Expression.Multiply)
                return "*";
            else if (Operation == Expression.Add)
                return "+";
            else if (Operation == Expression.Subtract)
                return "-";
            else if (Operation == Expression.Divide)
                return "/";
            else return "?";
        }
    }

    public class UnaryVectorOperation : VectorOperation
    {
        public readonly NAray Operand;
        public readonly NAray Result;
        public readonly MethodInfo Operation;

        public UnaryVectorOperation(NAray operand, NAray result, MethodInfo operation)
        {
            Operand = operand;
            Result = result;
            Operation = operation;
        }
    }

    public class AssignOperation : VectorOperation
    {
        public readonly NAray Left;
        public readonly NAray Right;

        public AssignOperation(NAray left, NAray right)
        {

        }
    }
}
