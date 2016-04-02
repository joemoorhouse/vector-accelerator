using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace VectorAccelerator.DeferredExecution.Expressions
{
    public class UnaryMathsExpression : Expression
    {
        public UnaryElementWiseOperation UnaryType { get; private set; }

        public VectorParameterExpression Operand { get; private set; }
        
        public override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Call;
            }
        }

        public override string ToString()
        {
            return string.Format(Prototype(), Operand);
        }

        public override Type Type
        {
            get
            {
                return Operand.Type;
            }
        }

        internal UnaryMathsExpression(UnaryElementWiseOperation unaryType, VectorParameterExpression operand)
        {
            UnaryType = unaryType;
            Operand = operand;
        }

        internal virtual string Prototype()
        {
            switch (UnaryType)
            {
                case UnaryElementWiseOperation.Exp:
                    return "exp({0})";
                case UnaryElementWiseOperation.SquareRoot:
                    return "sqrt({0})";
            }
            return "n/a";
        }
    } 
  
    public static class ExpressionExtended
    {
        public static UnaryMathsExpression MakeUnary(UnaryElementWiseOperation unaryType, VectorParameterExpression operand)
        {
            return new UnaryMathsExpression(unaryType, operand);
        }

        public static ScaleOffsetExpression<T> ScaleOffset<T>(VectorParameterExpression operand, T scale, T offset)
        {
            return new ScaleOffsetExpression<T>(operand, scale, offset);
        }
    }
}
