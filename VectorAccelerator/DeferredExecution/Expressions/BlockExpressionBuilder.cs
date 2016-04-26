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

        public void AddScaleOffsetOperation<T>(NArray<T> a, T scale, T offset, NArray<T> result)
        {
            _operations.Add(
                Expression.Assign(GetParameter<T>(result),
                ExpressionExtended.ScaleOffset<T>(GetParameter<T>(a), scale, offset))
                );
        }

        /// <summary>
        /// Efficient representation of product of two Expressions. This returns constant or parameter if possible, otherwise constructs
        /// a new assignment expression and returns the new local.  
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public Expression AddProductExpression(Expression a, Expression b)
        {   
            if (a is ConstantExpression<double> && b is ConstantExpression<double>)
            {
                return new ConstantExpression<double>(
                    (a as ConstantExpression<double>).Value *
                    (b as ConstantExpression<double>).Value);
            }
            else if (a is VectorParameterExpression && b is ConstantExpression<double>)
            {
                ConstantExpression<double> constant = b as ConstantExpression<double>;
                if (constant.Value == 1.0) return a;
                return AddLocalAssignment<double>(new ScaleOffsetExpression<double>(a as VectorParameterExpression, constant.Value, 0));
            }
            else if (b is VectorParameterExpression && a is ConstantExpression<double>)
            {
                ConstantExpression<double> constant = a as ConstantExpression<double>;
                if (constant.Value == 1.0) return b;
                return AddLocalAssignment<double>(new ScaleOffsetExpression<double>(b as VectorParameterExpression, constant.Value, 0));
            }
            else if (a is VectorParameterExpression && b is VectorParameterExpression)
            {
                return AddLocalAssignment<double>(Expression.MakeBinary(ExpressionType.Multiply, a, b));
            }
            else throw new NotImplementedException();
        }

        /// <summary>
        /// Efficient representation of sum of two Expressions. This returns constant or parameter if possible, otherwise constructs
        /// a new assignment expression and returns the new local.  
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public Expression AddAdditionExpression(Expression a, Expression b)
        {
            if (a is ConstantExpression<double> && b is ConstantExpression<double>)
            {
                return new ConstantExpression<double>(
                    (a as ConstantExpression<double>).Value +
                    (b as ConstantExpression<double>).Value);
            }
            else if (a is VectorParameterExpression && b is ConstantExpression<double>)
            {
                ConstantExpression<double> constant = b as ConstantExpression<double>;
                if (constant.Value == 0) return a;
                return AddLocalAssignment<double>(new ScaleOffsetExpression<double>(a as VectorParameterExpression, 0, constant.Value));
            }
            else if (b is VectorParameterExpression && a is ConstantExpression<double>)
            {
                ConstantExpression<double> constant = a as ConstantExpression<double>;
                if (constant.Value == 0) return b;
                return AddLocalAssignment<double>(new ScaleOffsetExpression<double>(b as VectorParameterExpression, 0, constant.Value));
            }
            else if (a is VectorParameterExpression && b is VectorParameterExpression)
            {
                return AddLocalAssignment<double>(Expression.MakeBinary(ExpressionType.Add, a, b));
            }
            else throw new NotImplementedException();
        }

        public VectorParameterExpression AddLocalAssignment<T>(Expression rhs)
        {
            ILocalNArray dummy;
            var lhs = CreateLocalOfLength<T>(_vectorLength, out dummy);
            _operations.Add(
                Expression.Assign(lhs, rhs));
            return lhs;
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

        private LocalNArrayVectorParameterExpression<T> CreateLocalOfLength<T>(int length, out ILocalNArray array)
        {
            NArray<T> localArray = null;
            if (typeof(T) == typeof(double)) localArray = new LocalNArray(_localParameters.Count, length) as NArray<T>;
            else if (typeof(T) == typeof(int)) localArray = new LocalNArrayInt(_localParameters.Count, length) as NArray<T>;
            array = localArray as ILocalNArray;
            var parameter = new LocalNArrayVectorParameterExpression<T>(localArray);
            _localParameters.Add(parameter);
            return parameter;
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
                VectorParameterExpression argument;
                // is this fast enough?
                if (!_argumentLookup.TryGetValue(array, out argument))
                {
                    argument = new ReferencingVectorParameterExpression<T>(array, ParameterType.Argument, _argumentLookup.Count);
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
            // an important optimisation: any vector-scalar or scalar-scalar operations are evaluated
            if (assignmentExpression.Right is BinaryExpression)
            {
                var binaryExpression = assignmentExpression.Right as BinaryExpression;
                var left = binaryExpression.Left as ReferencingVectorParameterExpression<double>;
                var right = binaryExpression.Right as ReferencingVectorParameterExpression<double>;
                if (left == null || right == null) return assignmentExpression; // only supports doubles for now
                if (left.Array.IsScalar)
                {
                    if (right.Array.IsScalar) // both are scalars
                    {
                        double value;
                        switch (binaryExpression.NodeType)
                        {
                            case (ExpressionType.Multiply):
                                value = left.Array.First() * right.Array.First(); break; 
                            default:
                                throw new NotImplementedException();
                        }
                        SetArrayToScalar<double>(assignmentExpression.Left, value);
                        return null; // now a null operation
                    }
                    else
                    {
                        // left is scalar, right is vector
                        switch (binaryExpression.NodeType)
                        {
                            case (ExpressionType.Multiply):
                                return Expression.Assign(assignmentExpression.Left, new ScaleOffsetExpression<double>(right, left.Array.First(), 0)); 
                            default:
                                throw new NotImplementedException();
                        }
                    }
                }
                else if (right.Array.IsScalar)
                {
                    // left is vector, right is scalar
                    switch (binaryExpression.NodeType)
                    {
                        case (ExpressionType.Multiply):
                            return Expression.AddAssign(assignmentExpression.Left, new ScaleOffsetExpression<double>(left, right.Array.First(), 0));
                        default:
                            throw new NotImplementedException();
                    }
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
