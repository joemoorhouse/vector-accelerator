using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace NArray.DeferredExecution.Expressions
{
    public abstract class BinaryExpression : Expression
    {
        private readonly Expression _left;
        private readonly Expression _right;

        internal BinaryExpression(Expression left, Expression right)
        {
            _left = left;
            _right = right;
        }

        /// <summary>
        /// Gets the left operand of the binary operation.
        /// </summary>
        public Expression Left { get { return _left; } }

        /// <summary>
        /// Gets the implementing method for the binary operation.
        /// </summary>
        //public MethodInfo Method { get; }

        /// <summary>
        /// Gets the right operand of the binary operation.
        /// </summary>
        public Expression Right { get { return _right; } }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", _left.ToString(), OperatorString(), _right.ToString());
        }

        protected abstract string OperatorString();
    }
}
