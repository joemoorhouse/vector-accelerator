using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace VectorAccelerator.DeferredExecution.Expressions
{
    public class ConstantExpression<T> : Expression
    {
        public readonly T Value;

        public override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Constant;
            }
        }

        public ConstantExpression(T value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
