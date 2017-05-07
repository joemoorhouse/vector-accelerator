using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace NArray.DeferredExecution.Expressions
{
    public class ConditionalExpression : Expression
    {
        private readonly Expression _condition;
        private readonly Expression _ifTrue;
        private readonly Expression _ifFalse;
        private readonly Type _type;

        internal ConditionalExpression(Expression condition, Expression ifTrue, Expression ifFalse, Type type)
        {
            _condition = condition;
            _ifTrue = ifTrue;
            _ifFalse = ifFalse;
            _type = type;
        }

        public override ExpressionType NodeType
        {
            get { return ExpressionType.Conditional; }
        }

        public override Type Type
        {
            get { return _type; }
        }

        public Expression Condition { get { return _condition; } }

        public Expression IfTrue { get { return _ifTrue; } }

        public Expression IfFalse { get { return _ifFalse; } }

        public override string ToString()
        {
            return string.Format("{0} ? {1} : {2}", _condition.ToString(), _ifTrue, _ifFalse);
        }
    }
}
