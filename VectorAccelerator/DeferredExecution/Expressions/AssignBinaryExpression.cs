using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorAccelerator.DeferredExecution.Expressions
{
    internal sealed class AssignBinaryExpression : BinaryExpression
    {
        public sealed override Type Type
        {
            get
            {
                return base.Left.Type;
            }
        }

        public sealed override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Assign;
            }
        }

        internal AssignBinaryExpression(Expression left, Expression right)
            : base(left, right)
        {
        }

        protected override string OperatorString()
        {
            return "=";
        }
    }
}
