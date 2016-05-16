using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using VectorAccelerator.NArrayStorage;

namespace VectorAccelerator.DeferredExecution.Expressions
{
    public class BlockExpressionBuilder
    {
        List<VectorParameterExpression> _localParameters = new List<VectorParameterExpression>();
        Dictionary<INArray, VectorParameterExpression> _argumentLookup = new Dictionary<INArray, VectorParameterExpression>(); 
        List<BinaryExpression> _operations = new List<BinaryExpression>();
        int _vectorLength = -1;

        public VectorBlockExpression ToBlock()
        {
            return new VectorBlockExpression()
            {
                Operations = _operations, 
                ArgumentParameters = _argumentLookup.Values.ToArray(), 
                LocalParameters = _localParameters };
        }

        public int VectorLength
        {
            get { return _vectorLength; }
            set { _vectorLength = value; }
        }

        public void AddAssign<T>(NArray<T> result, NArray<T> a)
        {
            _operations.Add(
                Expression.Assign(GetParameter<T>(result), GetParameter<T>(a)));
        }

        public void AddBinaryElementWiseOperation<T>(NArray<T> a, NArray<T> b,
            NArray<T> result, ExpressionType operation)
        {
            AddBinaryElementWiseOperation<T>(
                GetParameter<T>(a), GetParameter<T>(b), GetParameter<T>(result), operation);
        }

        public void AddBinaryElementWiseOperation<T>(VectorParameterExpression a, VectorParameterExpression b,
            VectorParameterExpression result, ExpressionType operation)
        {
            _operations.Add(
                Expression.Assign(result,
                Expression.MakeBinary(operation, a, b))
                );
        }

        public void AddUnaryElementWiseOperation<T>(NArray<T> a,
            NArray<T> result, UnaryElementWiseOperation operation)
        {
            _operations.Add(
                Expression.Assign(GetParameter<T>(result),
                ExpressionExtended.MakeUnary(operation, GetParameter<T>(a)))
                );
        }

        public void AddScaleInverseOperation<T>(NArray<T> a, T scale, NArray<T> result)
        {
            _operations.Add(
                Expression.Assign(GetParameter<T>(result),
                ExpressionExtended.ScaleInverse<T>(GetParameter<T>(a), scale))
                );
        }

        public void AddScaleOffsetOperation<T>(NArray<T> a, T scale, T offset, NArray<T> result)
        {
            _operations.Add(
                Expression.Assign(GetParameter<T>(result),
                ExpressionExtended.ScaleOffset<T>(GetParameter<T>(a), scale, offset))
                );
        }

        /// <summary>
        /// Adds a product expression efficiently; assumes that scalar independent variables can be combined
        /// i.e. that any differentiation has already occurred
        /// </summary>
        public VectorParameterExpression AddProductExpression(Expression a,
            Expression b)
        {
            var paramA = a as ReferencingVectorParameterExpression<double>;
            var paramB = b as ReferencingVectorParameterExpression<double>;
            if (paramA == null || paramB == null) throw new NotImplementedException();
            if (paramA.IsScalar && paramB.IsScalar)
            {
                return new ConstantExpression<double>(
                    paramA.ScalarValue * paramB.ScalarValue);
            }
            else if (paramA.IsScalar && paramA.ScalarValue == 1)
            {
                return paramB;
            }
            else if (paramB.IsScalar && paramB.ScalarValue == 1)
            {
                return paramA;
            }
            else
            {
                return AddLocalAssignment<double>(Expression.MakeBinary(ExpressionType.Multiply, paramA, paramB));
            }
        }

        /// <summary>
        /// Adds a sum expression efficiently; assumes that scalar independent variables can be combined
        /// i.e. that any differentiation has already occurred
        /// </summary>
        public VectorParameterExpression AddAdditionExpression(Expression a, 
            Expression b)
        {
            var paramA = a as ReferencingVectorParameterExpression<double>;
            var paramB = b as ReferencingVectorParameterExpression<double>;
            if (paramA == null || paramB == null) throw new NotImplementedException();
            if (paramA.IsScalar && paramB.IsScalar)
            {
                return new ConstantExpression<double>(
                    paramA.ScalarValue +
                    paramB.ScalarValue);
            }
            else if (paramA.IsScalar && paramA.ScalarValue == 0)
            {
                return paramB;
            }
            else if (paramB.IsScalar && paramB.ScalarValue == 0)
            {
                return paramA;
            }
            else
            {
                return AddLocalAssignment<double>(Expression.MakeBinary(ExpressionType.Add, paramA, paramB));
            }
        }

        /// <summary>
        /// Adds a division and negation expression efficiently; assumes that scalar independent variables can be combined
        /// i.e. that any differentiation has already occurred
        /// </summary>
        public Expression AddNegateDivideExpression(VectorParameterExpression a, VectorParameterExpression b)
        {
            var paramA = a as ReferencingVectorParameterExpression<double>;
            var paramB = b as ReferencingVectorParameterExpression<double>;
            if (paramA.IsScalar && paramB.IsScalar)
            {
                return new ConstantExpression<double>(
                    -paramA.ScalarValue /
                    paramB.ScalarValue);
            }
            var newLocal = AddLocalAssignment<double>(Expression.Divide(a, b));
            return AddLocalAssignment<double>(new ScaleOffsetExpression<double>(newLocal, -1, 0));
        }

        public VectorParameterExpression AddLocalAssignment<T>(Expression rhs)
        {
            ILocalNArray dummy;
            var lhs = CreateLocalOfLength<T>(_vectorLength, out dummy);
            _operations.Add(
                Expression.Assign(lhs, rhs));
            return lhs;
        }

        public VectorParameterExpression AddLocalAssignment<T>(T scalarValue, Expression rhs)
        {
            ILocalNArray dummy;
            var lhs = CreateScalarLocal<T>(scalarValue, out dummy);
            _operations.Add(
                Expression.Assign(lhs, rhs));
            return lhs;
        }

        public ILocalNArray CreateScalarLocal<T>(T value)
        {
            ILocalNArray array;
            CreateScalarLocal<T>(value, out array);
            return array;
        }

        public ILocalNArray CreateLocal<T>()
        {
            return CreateLocalOfLength<T>(_vectorLength);
        }

        private ILocalNArray CreateLocalOfLength<T>(int length)
        {
            ILocalNArray array;
            CreateLocalOfLength<T>(length, out array);
            return array;
        }

        private ReferencingVectorParameterExpression<T> CreateLocalOfLength<T>(int length, out ILocalNArray array)
        {
            NArray<T> localArray = null;
            var index = NextLocalIndex();
            if (typeof(T) == typeof(double)) localArray = new LocalNArray(index, length) as NArray<T>;
            else if (typeof(T) == typeof(int)) localArray = new LocalNArrayInt(index, length) as NArray<T>;
            array = localArray as ILocalNArray;
            var parameter = new ReferencingVectorParameterExpression<T>(localArray, ParameterType.Local, index);
            _localParameters.Add(parameter);
            return parameter;
        }

        private ReferencingVectorParameterExpression<T> CreateScalarLocal<T>(T value, out ILocalNArray array)
        {
            NArray<T> localArray = null;
            var index = NextLocalIndex();
            if (typeof(T) == typeof(double)) localArray = new LocalNArray(index, Convert.ToDouble(value)) as NArray<T>;
            else throw new NotImplementedException();
            array = localArray as ILocalNArray;
            var parameter = new ReferencingVectorParameterExpression<T>(localArray.First(), ParameterType.Local, index);
            _localParameters.Add(parameter);
            return parameter;
        }

        private int NextLocalIndex()
        {
            return _localParameters.Count == 0 ? 0 : _localParameters.Last().Index + 1; // we assume only that list is in increasing order: we can thereby renumber
        }

        public VectorParameterExpression GetParameter<T>(NArray<T> array)
        {
            var local = array as ILocalNArray;
            if (local != null)
            {
                return _localParameters[local.Index] as VectorParameterExpression;
            }
            else
            {
                // if the array is a scalar and not an independent variable of any derivative calculation, we do not care where it came from; do not 
                // add to argument list
                if (array.IsScalar && !array.IsIndependentVariable) return new ConstantExpression<T>(array.First());
                VectorParameterExpression argument;
                // is this fast enough?
                if (!_argumentLookup.TryGetValue(array, out argument))
                {
                    argument = array.IsScalar ? new ReferencingVectorParameterExpression<T>(array.First(), ParameterType.Argument, _argumentLookup.Count)
                        : new ReferencingVectorParameterExpression<T>(array, ParameterType.Argument, _argumentLookup.Count);
                    _argumentLookup.Add(array, argument);
                }
                return argument;
            }
        }

        /// <summary>
        /// Make locals numbering follow on from arguments
        /// </summary>
        public void UpdateLocalsNumbering()
        {
            for (int i = 0; i < _localParameters.Count; ++i)
            {
                _localParameters[i].Index = i + _argumentLookup.Count;
            }
        }

        #region Simplification

        public BinaryExpression SimplifyOperation(BinaryExpression assignmentExpression)
        {
            // if operation is a scalar, nothing to do (result is already calculated)
            if ((assignmentExpression.Left as ReferencingVectorParameterExpression<double>).IsScalar) return null;

            // if one side of operation is scalar, can be simplified:
            if (assignmentExpression.Right is BinaryExpression)
            {
                var binaryExpression = assignmentExpression.Right as BinaryExpression;
                var left = binaryExpression.Left as ReferencingVectorParameterExpression<double>;
                var right = binaryExpression.Right as ReferencingVectorParameterExpression<double>;
                if (left == null || right == null) return assignmentExpression; // only supports doubles for now
                if (left.IsScalar)
                {
                    if (right.IsScalar) // both are scalars
                    {
                        throw new NotImplementedException(); // should not happen
                    }
                    else
                    {
                        // left is scalar, right is vector
                        switch (binaryExpression.NodeType)
                        {
                            case (ExpressionType.Add):
                                return Expression.Assign(assignmentExpression.Left, new ScaleOffsetExpression<double>(right, 1, left.ScalarValue));
                            case (ExpressionType.Subtract):
                                return Expression.Assign(assignmentExpression.Left, new ScaleOffsetExpression<double>(right, -1, left.ScalarValue));
                            case (ExpressionType.Multiply):
                                return Expression.Assign(assignmentExpression.Left, new ScaleOffsetExpression<double>(right, left.ScalarValue, 0));
                            case (ExpressionType.Divide):
                                return Expression.Assign(assignmentExpression.Left, new ScaleInverseExpression<double>(right, left.ScalarValue));
                            default:
                                throw new NotImplementedException();
                        }
                    }
                }
                else if (right.IsScalar)
                {
                    // left is vector, right is scalar
                    switch (binaryExpression.NodeType)
                    {
                        case (ExpressionType.Add):
                            return Expression.Assign(assignmentExpression.Left, new ScaleOffsetExpression<double>(left, 1, right.ScalarValue));
                        case (ExpressionType.Subtract):
                            return Expression.Assign(assignmentExpression.Left, new ScaleOffsetExpression<double>(left, 1, -right.ScalarValue));
                        case (ExpressionType.Multiply):
                            return Expression.Assign(assignmentExpression.Left, new ScaleOffsetExpression<double>(left, right.ScalarValue, 0));
                        case (ExpressionType.Divide):
                            return Expression.Assign(assignmentExpression.Left, new ScaleOffsetExpression<double>(left, 1 / right.ScalarValue, 0));
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            else if (assignmentExpression.Right is UnaryMathsExpression)
            {
                var unaryMathsExpression = assignmentExpression.Right as UnaryMathsExpression;
                var operand = unaryMathsExpression.Operand as ReferencingVectorParameterExpression<double>;
                if (operand.IsScalar) 
                {
                    throw new NotImplementedException(); // should not happen
                }
            }
            return assignmentExpression;
        }

        public static NArray<T> GetArray<T>(Expression expression)
        {
            return (expression as ReferencingVectorParameterExpression<T>).Array;
        }

        public static void SetArrayToScalar<T>(Expression expression, T scalarValue)
        {
            (expression as ReferencingVectorParameterExpression<T>).Array.Storage = new ManagedStorage<T>(scalarValue);
        }

        private static bool IsScalar<T>(Expression parameter)
        {
            return (parameter as ReferencingVectorParameterExpression<T>).Array.IsScalar;
        }

        private static T ScalarValue<T>(Expression parameter)
        {
            return (parameter as ReferencingVectorParameterExpression<T>).Array.First();
        }

        #endregion
    }
}
