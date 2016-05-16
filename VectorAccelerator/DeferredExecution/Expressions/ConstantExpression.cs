using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace VectorAccelerator.DeferredExecution.Expressions
{
    public class ConstantExpression<T> : ReferencingVectorParameterExpression<T>
    {
        public override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Constant;
            }
        }

        public ConstantExpression(T value)
            : base(value, ParameterType.Constant, -1) { }
    }
}
