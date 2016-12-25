using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace NArray.DeferredExecution.Expressions
{
    public class UnaryMathsExpression : Expression
    {
        public UnaryElementWiseOperations UnaryType { get; private set; }

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

        internal UnaryMathsExpression(UnaryElementWiseOperations unaryType, VectorParameterExpression operand)
        {
            UnaryType = unaryType;
            Operand = operand;
        }

        internal virtual string Prototype()
        {
            switch (UnaryType)
            {
                case UnaryElementWiseOperations.Negate:
                    return "-({0})";
                case UnaryElementWiseOperations.Exp:
                    return "exp({0})";
                case UnaryElementWiseOperations.SquareRoot:
                    return "sqrt({0})";
                case UnaryElementWiseOperations.Log:
                    return "log({0})";
                case UnaryElementWiseOperations.CumulativeNormal:
                    return "cdf({0})";
            }
            return "n/a";
        }
    } 
  
    public static class ExpressionExtended
    {
        public static UnaryMathsExpression MakeUnary(UnaryElementWiseOperations unaryType, VectorParameterExpression operand)
        {
            return new UnaryMathsExpression(unaryType, operand);
        }

        public static ScaleInverseExpression ScaleInverse(VectorParameterExpression operand, double scale)
        {
            return new ScaleInverseExpression(operand, scale);
        }

        public static ScaleOffsetExpression ScaleOffset(VectorParameterExpression operand, double scale, double offset)
        {
            return new ScaleOffsetExpression(operand, scale, offset);
        }
    }
}
