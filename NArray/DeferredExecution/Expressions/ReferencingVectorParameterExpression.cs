using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace NArray.DeferredExecution.Expressions
{
    /// <summary>
    /// A parameter expression that references an NArray that acts as identifier.
    /// </summary>
    public class ReferencingVectorParameterExpression : VectorParameterExpression
    {
        public readonly NArray Array;
        public readonly double ScalarValue;
        public readonly bool IsScalar;

        public override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Parameter;
            }
        }

        public ReferencingVectorParameterExpression(NArray array, ParameterType parameterType, int index)
            : base(typeof(NArray), parameterType, index)
        {
            Array = array;
            IsScalar = false;
        }

        public ReferencingVectorParameterExpression(double scalarValue, ParameterType parameterType, int index)
            : base(typeof(NArray), parameterType, index)
        {
            Array = null;
            ScalarValue = scalarValue;
            IsScalar = true;
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
