using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NArray.DeferredExecution.Expressions
{
    public class ScaleInverseExpression : UnaryMathsExpression
    {
        public readonly double Scale;

        internal ScaleInverseExpression(VectorParameterExpression operand, double scale)
            : base(UnaryElementWiseOperations.ScaleInverse, operand)
        {
            Scale = scale;
        }

        internal override string Prototype()
        {
            return Scale.ToString() + " / {0}";
        }
    }
}
