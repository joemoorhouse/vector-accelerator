using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorAccelerator.DeferredExecution.Expressions
{
    public class ScaleOffsetExpression<T> : UnaryMathsExpression
    {
        public readonly VectorParameterExpression Operand;
        public readonly T Scale;
        public readonly T Offset;

        internal ScaleOffsetExpression(VectorParameterExpression operand, T scale, T offset)
            : base(UnaryElementWiseOperation.ScaleOffset, operand)
        {
            Operand = operand;
            Scale = scale;
            Offset = offset;
        }

        internal override string Prototype()
        {
            if (IsZero(Offset)) return Scale.ToString() + " * {0}";
            else if (IsOne(Scale)) return Offset.ToString() + " + {0}";
            else return Offset.ToString() + " + " + Scale.ToString() + " * {0}";
        }

        private bool IsZero(T value)
        {
            if (typeof(T) == typeof(double)) return (System.Convert.ToDouble(value) == 0);
            else if (typeof(T) == typeof(float)) return (System.Convert.ToSingle(value) == 0f);
            else return false;
        }

        private bool IsOne(T value)
        {
            if (typeof(T) == typeof(double)) return (System.Convert.ToDouble(value) == 1);
            else if (typeof(T) == typeof(float)) return (System.Convert.ToSingle(value) == 1f);
            else return false;
        }
    }
}
