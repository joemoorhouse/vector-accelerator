using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace NArray.DeferredExecution.Expressions
{
    public class ConstantExpression : ReferencingVectorParameterExpression
    {
        public override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Constant;
            }
        }

        public ConstantExpression(double value)
            : base(value, ParameterType.Constant, -1) { }
    }
}
