using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace VectorAccelerator.DeferredExecution.Expressions
{
    /// <summary>
    /// A parameter expression that references an NArray that acts as identifier.
    /// </summary>
    public class ReferencingVectorParameterExpression<T> : VectorParameterExpression
    {
        public readonly NArray<T> Array;
        public readonly T ScalarValue;
        public readonly bool IsScalar;

        public override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Parameter;
            }
        }

        public ReferencingVectorParameterExpression(NArray<T> array, ParameterType parameterType, int index)
            : base(GetType<T>(), parameterType, index)
        {
            Array = array;
            IsScalar = false;
        }

        public ReferencingVectorParameterExpression(T scalarValue, ParameterType parameterType, int index)
            : base(GetType<T>(), parameterType, index)
        {
            Array = null;
            ScalarValue = scalarValue;
            IsScalar = true;
        }

        private static Type GetType<S>()
        {
            if (typeof(S) == typeof(double)) return typeof(NArray);
            else return typeof(NArray<S>);
        }

        public override string ToString()
        {
            if (IsScalar)
            {
                if (Name == string.Empty) return ScalarValue.ToString();
                else return String.Format("{0}[{1}]", Name, ScalarValue);
            }
            else return Name;
        }
    }
}
