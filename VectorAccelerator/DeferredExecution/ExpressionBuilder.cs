using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace VectorAccelerator.DeferredExecution
{
    /// <summary>
    /// The entire kernel is represented as a single BlockExpression. Through this class we add Expressions
    /// progressively and finally create the BlockExpression.
    /// </summary>
    public class ExpressionBuilder
    {   
        List<Expression> _expressions = new List<Expression>();
        List<VectorOperation> _vectorOperations = new List<VectorOperation>();
        List<ParameterExpression> _localVariables = new List<ParameterExpression>();
        Dictionary<NAray, ParameterExpression> _parameters = new Dictionary<NAray, ParameterExpression>();
        SortedDictionary<double, int> _constantIndices = new SortedDictionary<double, int>();
        List<ConstantExpression> _constants = new List<ConstantExpression>();

        public IList<Expression> Expressions { get { return _expressions.AsReadOnly(); } }

        public IList<VectorOperation> VectorOperations { get { return _vectorOperations.AsReadOnly(); } }

        public IDictionary<NAray, ParameterExpression> Parameters { get { return _parameters; } }

        public void Assign(NAray operand1, NAray operand2)
        {
            _vectorOperations.Add(new AssignOperation(operand1, operand2));
            _expressions.Add(
                Expression.Assign(GetNArray(operand1), GetNArray(operand2))
                    );
        }

        public NAray UnaryExpression(NAray operand, Func<Expression, UnaryExpression> operation)
        {
            NAray result;
            ParameterExpression parameterExpression;
            NewLocal(out result, out parameterExpression);
            _expressions.Add(
                Expression.Assign(parameterExpression,
                    operation(GetNArray(operand))
                    ));
            return result;
        }

        public NAray MethodCallExpression(NAray operand, MethodInfo method)
        {
            NAray result;
            ParameterExpression parameterExpression;
            NewLocal(out result, out parameterExpression);
            _expressions.Add(
                Expression.Assign(parameterExpression,
                    Expression.Call(method, GetNArray(operand))
                    ));
            return result;
        }

        public NAray BinaryExpression(NAray operand1, NAray operand2, Func<Expression, Expression, BinaryExpression> operation)
        {
            NAray result;
            ParameterExpression parameterExpression;
            NewLocal(out result, out parameterExpression);

            _vectorOperations.Add(new BinaryVectorOperation(operand1, operand2, result, operation));

            Expression arg1, arg2;
            arg1 = (operand1.IsScalar) ? (Expression)GetConstant(operand1.First()) : (Expression)GetNArray(operand1);
            arg2 = (operand2.IsScalar) ? (Expression)GetConstant(operand2.First()) : (Expression)GetNArray(operand2);

            _expressions.Add(
                Expression.Assign(parameterExpression,
                operation(arg1, arg2)
            ));
           
            return result;
        }

        private void NewLocal(out NAray newLocal, out ParameterExpression newLocalExpression)
        {
            newLocal = new LocalNArray(_localVariables.Count);
            newLocalExpression = Expression.Parameter(typeof(double), "local" + _localVariables.Count);
            _localVariables.Add(newLocalExpression); 
        }

        private ConstantExpression GetConstant(double constant)
        {
            int constantIndex;
            if (!_constantIndices.TryGetValue(constant, out constantIndex))
            {
                constantIndex = _constantIndices.Count;
                _constantIndices.Add(constant, constantIndex);
                _constants.Add(Expression.Constant(constant, typeof(double)));
            }
            return _constants[constantIndex];
        }

        private ParameterExpression GetNArray(NAray NArray)
        {
            if (NArray is LocalNArray) // this is a local variable
            {
                return _localVariables[(NArray as LocalNArray).Index];
            }
            else
            {
                ParameterExpression expression;
                if (!_parameters.TryGetValue(NArray, out expression))
                {
                    var newParameterExpression = Expression.Parameter(typeof(double), "parameter" + _parameters.Count);
                    _parameters.Add(NArray, newParameterExpression);
                }
                return _parameters[NArray];
            }
        }

        public BlockExpression BuildBlock()
        {
            BlockExpression block = Expression.Block(
                _parameters.Values.Concat(_localVariables).ToArray(), _expressions);
            return block;
        }

        public string[] DebugViewExpressions()
        {
            return _expressions.Select(e => e.ToString()).ToArray();
        }
    }
}
