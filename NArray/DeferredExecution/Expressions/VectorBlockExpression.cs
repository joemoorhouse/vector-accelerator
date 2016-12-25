using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace NArray.DeferredExecution.Expressions
{    
    public class VectorBlockExpression : Expression
    {        
        public IReadOnlyList<VectorParameterExpression> LocalParameters;
        public IReadOnlyList<VectorParameterExpression> ArgumentParameters;
        public IReadOnlyList<BinaryExpression> Operations;

        public override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Block;
            }
        }

        public override Type Type
        {
            get
            {
                return null;
            }
        }
    }
}
