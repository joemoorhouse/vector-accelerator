using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace NArray.DeferredExecution.Expressions
{
    /// <summary>
    /// Argument = input of calculation. Scalars may be arguments, but only if used as an independent variable
    /// Local = intermediate result of calculation. Scalars may be locals, but only if the scalar is an independent variable
    /// Constant = scalar that is not an independent variable
    /// </summary>
    public enum ParameterType {  Local, Argument, Constant }; 
    
    public abstract class VectorParameterExpression : Expression
    {
        private readonly Type _type;
        private int _index;
        private readonly ParameterType _parameterType;

        public int Index
        {
            get { return _index; }
            internal set { _index = value; }
        }
        
        public string Name
        {
            get
            {
                if (_index == -1) return string.Empty;
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
