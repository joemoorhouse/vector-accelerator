using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace NArray.DeferredExecution.Expressions
{
    public sealed class UnaryExpression : Expression
    {
        private readonly Expression _operand;
        private readonly MethodInfo _method;
        private readonly ExpressionType _nodeType;
        private readonly Type _type;

        public sealed override Type Type
        {
            get
            {
                return this._type;
            }
        }

        public sealed override ExpressionType NodeType
        {
            get
            {
                return this._nodeType;
            }
        }

        public Expression Operand
        {
            get
            {
                return this._operand;
            }
        }

        public MethodInfo Method
        {
            get
            {
                return this._method;
            }
        }

        internal UnaryExpression(ExpressionType nodeType, Expression expression, Type type, MethodInfo method)
        {
            this._operand = expression;
            this._method = method;
            this._nodeType = nodeType;
            this._type = type;
        }
    }
}
