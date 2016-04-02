using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace VectorAccelerator.DeferredExecution.Expressions
{
    /// <summary>
    /// Special type of Block expression that represents a vector operation
    /// </summary>
    public class VectorOperationExpression : Expression
    {
        public readonly VectorParameterExpression Result;

        public override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Block;
            }
        }
        
        public VectorOperationExpression(VectorParameterExpression result)
        {
            Result = result;
        }

        public virtual Expression Differentiate(Expression operand)
        {
            return null;
        }
    }
}
