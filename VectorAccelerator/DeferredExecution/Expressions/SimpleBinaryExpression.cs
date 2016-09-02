using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorAccelerator.DeferredExecution.Expressions
{
    public class SimpleBinaryExpression : BinaryExpression
    {
        // fields would be in SimpleBinaryExpression
        private readonly ExpressionType _nodeType;
        private readonly Type _type;

        internal SimpleBinaryExpression(ExpressionType nodeType, Expression left, Expression right, Type type) : base(left, right)
        {
            _nodeType = nodeType;
            _type = type;
        }

        public override ExpressionType NodeType
        {
            get { return _nodeType; }
        }

        public override Type Type
        {
            get { return _type; }
        }

        protected override string OperatorString()
        {
            switch (_nodeType)
            {
                case ExpressionType.Add:
                    return "+";
                case ExpressionType.Subtract:
                    return "-";
                case ExpressionType.Multiply:
                    return "*";
                case ExpressionType.Divide:
                    return "/";
                default:
                    return "N/A";
            }
        }
    }
}
