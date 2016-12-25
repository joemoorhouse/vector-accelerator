using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NArray.DeferredExecution.Expressions
{
    public class ScaleOffsetExpression : UnaryMathsExpression
    {
        public new readonly VectorParameterExpression Operand;
        public readonly double Scale;
        public readonly double Offset;

        internal ScaleOffsetExpression(VectorParameterExpression operand, double scale, double offset)
            : base(UnaryElementWiseOperations.ScaleOffset, operand)
        {
            Operand = operand;
            Scale = scale;
            Offset = offset;
        }

        internal override string Prototype()
        {
            if (Offset == 0) return Scale.ToString() + " * {0}";
            else if (Scale == 1) return Offset.ToString() + " + {0}";
            else return Offset.ToString() + " + " + Scale.ToString() + " * {0}";
        }
    }
}
