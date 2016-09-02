using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorAccelerator.DeferredExecution.Expressions
{
    public class ScaleInverseExpression<T> : UnaryMathsExpression
    {
        public readonly T Scale;

        internal ScaleInverseExpression(VectorParameterExpression operand, T scale)
            : base(UnaryElementWiseOperation.ScaleInverse, operand)
        {
            Scale = scale;
        }

        internal override string Prototype()
        {
            return Scale.ToString() + " / {0}";
        }
    }
}
