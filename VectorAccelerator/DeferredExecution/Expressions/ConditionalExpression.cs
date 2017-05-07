using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorAccelerator.DeferredExecution.Expressions
{
    public class ConditionalExpression : Expression
    {
        private readonly Expression _test;

        private readonly Expression _true;

        private readonly Expression _false;

        /// <summary>Returns the node type of this expression. Extension nodes should return <see cref="F:System.Linq.Expressions.ExpressionType.Extension" /> when overriding this method.</summary>
        /// <returns>The <see cref="T:System.Linq.Expressions.ExpressionType" /> of the expression.</returns>
        public sealed override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Conditional;
            }
        }

        /// <summary>Gets the static type of the expression that this <see cref="T:System.Linq.Expressions.Expression" /> represents.</summary>
        /// <returns>The <see cref="P:System.Linq.Expressions.ConditionalExpression.Type" /> that represents the static type of the expression.</returns>
        public override Type Type
        {
            get
            {
                return this.IfTrue.Type;
            }
        }

        /// <summary>Gets the test of the conditional operation.</summary>
        /// <returns>An <see cref="T:System.Linq.Expressions.Expression" /> that represents the test of the conditional operation.</returns>
        public Expression Test
        {
            get
            {
                return this._test;
            }
        }

        /// <summary>Gets the expression to execute if the test evaluates to true.</summary>
        /// <returns>An <see cref="T:System.Linq.Expressions.Expression" /> that represents the expression to execute if the test is true.</returns>
        public Expression IfTrue
        {
            get
            {
                return this._true;
            }
        }

        /// <summary>Gets the expression to execute if the test evaluates to false.</summary>
        /// <returns>An <see cref="T:System.Linq.Expressions.Expression" /> that represents the expression to execute if the test is false.</returns>
        public Expression IfFalse
        {
            get
            {
                return this._false;
            }
        }

        internal ConditionalExpression(Expression test, Expression ifTrue, Expression ifFalse)
        {
            this._test = test;
            this._true = ifTrue;
            this._false = ifFalse;
        }

        public override string ToString()
        {
            return string.Format("IIF({0}, {1}, {2})", _test.ToString(), _true.ToString(), _false.ToString());
        }
    }
}
