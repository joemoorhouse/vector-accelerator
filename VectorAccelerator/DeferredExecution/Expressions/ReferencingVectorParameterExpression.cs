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
        }

        private static Type GetType<T>()
        {
            if (typeof(T) == typeof(double)) return typeof(NArray);
            else return typeof(NArray<T>);
        }
    }
}
