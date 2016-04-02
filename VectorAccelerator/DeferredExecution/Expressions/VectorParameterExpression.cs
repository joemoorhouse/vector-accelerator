using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace VectorAccelerator.DeferredExecution.Expressions
{
    public enum ParameterType {  Local, Argument };
    
    public abstract class VectorParameterExpression : Expression
    {
        private Type _type;
        private int _index;
        private ParameterType _parameterType;

        public int Index
        {
            get { return _index; }
            internal set { _index = value; }
        }
        
        public string Name
        {
            get
            {
                return string.Format("{0}{1}",
                    _parameterType == ParameterType.Argument ? "arg" : "local",
                    _index);
            }
        }

        public ParameterType ParameterType
        {
            get { return _parameterType; }
        }

        public override string ToString()
        {
            return Name;
        }

        public override Type Type
        {
            get
            {
                return _type;
            }
        }

        public override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Parameter;
            }
        }

        public VectorParameterExpression(Type type, ParameterType parameterType, int index) 
        {
            _type = type;
            _parameterType = parameterType;
            _index = index;
        }
    }
}
