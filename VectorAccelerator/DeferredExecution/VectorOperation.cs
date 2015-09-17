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

        public virtual VectorOperation Clone(Func<NArray, NArray> transform)
        {
            return new VectorOperation() { Result = transform(Result) };
        }

        public virtual IList<NArray> Operands()
        {
            return null;
        }
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

        public override VectorOperation Clone(Func<NArray, NArray> transform)
        {
            return new BinaryVectorOperation(transform(Operand1), transform(Operand2), transform(Result), Operation);
        }

        public override IList<NArray> Operands()
        {
            return new List<NArray> { Operand1, Operand2 };
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

        public override VectorOperation Clone(Func<NArray, NArray> transform)
        {
            return new UnaryVectorOperation(transform(Operand), transform(Result), Operation);
        }

        public override IList<NArray> Operands()
        {
            return new List<NArray> { Operand };
        }
    }

    public class ScaleOffsetOperation : UnaryVectorOperation
    {
        public readonly double Scale;
        public readonly double Offset;
        
        public ScaleOffsetOperation(NArray operand, NArray result, double scale, double offset,
            Action<NArray, double, double, NArray> scaleOffsetOperation)
            : base(operand, result, (op, res) => scaleOffsetOperation(op, scale, offset, res))
        {
            Scale = scale;
            Offset = offset;
        }

        public ScaleOffsetOperation(NArray operand, NArray result, double scale, double offset,
            Action<NArray, NArray> scaleOffsetOperation) : base(operand, result, scaleOffsetOperation)
        {
            Scale = scale;
            Offset = offset;
        }

        public override string ToString()
        {
            return string.Join(" ", Result.ToString(), "=", Scale.ToString(), "*", Operand.ToString(), "+", Offset.ToString());
        }

        public override VectorOperation Clone(Func<NArray, NArray> transform)
        {
            return new ScaleOffsetOperation(transform(Operand), transform(Result), Scale, Offset, Operation);
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

        public override string ToString()
        {
            return string.Join(" ", Left.ToString(), "=", Right.ToString());
        }
    }
}
