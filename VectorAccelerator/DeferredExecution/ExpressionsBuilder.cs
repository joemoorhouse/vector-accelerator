using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq.Expressions;
using VectorAccelerator.LinearAlgebraProviders;

namespace VectorAccelerator.DeferredExecution
{
    /// <summary>
    /// The entire kernel is represented as a single BlockExpression. Through this class we add Expressions
    /// progressively and finally create the BlockExpression.
    /// </summary>
    public class ExpressionsBuilder
    {   
        List<Expression> _expressions = new List<Expression>();
        List<ParameterExpression> _localVariables = new List<ParameterExpression>();
        Dictionary<NArray, ParameterExpression> _parameters = new Dictionary<NArray, ParameterExpression>();
        SortedDictionary<double, int> _constantIndices = new SortedDictionary<double, int>();
        List<ConstantExpression> _constants = new List<ConstantExpression>();
        int _vectorsLength = -1;

        public IList<Expression> Expressions { get { return _expressions.AsReadOnly(); } }

        public IDictionary<NArray, ParameterExpression> Parameters { get { return _parameters; } }

        public int VectorsLength { get { return _vectorsLength; } }

        public void Assign(NArray operand1, NArray operand2)
        {
            _expressions.Add(
                Expression.Assign(GetNArray(operand1), GetNArray(operand2))
                    );
        }

        public NArray UnaryExpression(NArray operand, Func<Expression, UnaryExpression> operation)
        {
            CheckNArrays(operand);
            NArray result;
            ParameterExpression parameterExpression;
            NewLocal(out result, out parameterExpression);
            _expressions.Add(
                Expression.Assign(parameterExpression,
                    operation(GetNArray(operand))
                    ));
            return result;
        }

        public NArray MethodCallExpression(NArray operand, MethodInfo method)
        {
            CheckNArrays(operand);
            NArray result;
            ParameterExpression parameterExpression;
            NewLocal(out result, out parameterExpression);
            _expressions.Add(
                Expression.Assign(parameterExpression,
                    Expression.Call(method, GetNArray(operand))
                    ));
            return result;
        }

        public NArray BinaryExpression(NArray operand1, NArray operand2, 
            Func<Expression, Expression, BinaryExpression> operation)
        {
            CheckNArrays(operand1, operand2);
            NArray result;
            ParameterExpression parameterExpression;
            NewLocal(out result, out parameterExpression);

            Expression arg1, arg2;
            arg1 = (operand1.IsScalar) ? (Expression)GetConstant(operand1.First()) : (Expression)GetNArray(operand1);
            arg2 = (operand2.IsScalar) ? (Expression)GetConstant(operand2.First()) : (Expression)GetNArray(operand2);

            _expressions.Add(
                Expression.Assign(parameterExpression,
                operation(arg1, arg2)
            ));
           
            return result;
        }

        private void NewLocal(out NArray newLocal, out ParameterExpression newLocalExpression)
        {
            newLocal = new LocalNArray(_localVariables.Count, _vectorsLength);
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

        private ParameterExpression GetNArray(NArray NArray)
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

        private void CheckNArrays(params NArray[] arrays)
        {
            if (_vectorsLength == -1) _vectorsLength = arrays.First().Length;
            foreach (var array in arrays)
            {
                if (array.Length != _vectorsLength && !(array is LocalNArray))
                    throw new ArithmeticException("array length mismatch");
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
