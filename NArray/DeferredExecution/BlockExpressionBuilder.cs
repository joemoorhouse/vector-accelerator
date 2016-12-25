using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NArray;
using NArray.Interfaces;
using NArray.DeferredExecution.Expressions;

namespace NArray.DeferredExecution
{
    public delegate LocalNArray NewLocalNArray(int index, int length);
    public delegate LocalNArray NewLocalScalarNArray(int index, double scalarValue);

    public class BlockExpressionBuilder
    {
        List<VectorParameterExpression> _localParameters = new List<VectorParameterExpression>();
        Dictionary<NArray, VectorParameterExpression> _argumentLookup = new Dictionary<NArray, VectorParameterExpression>();
        HashSet<NArray> _independentVariables = new HashSet<NArray>(); 
        List<BinaryExpression> _operations = new List<BinaryExpression>();
        int _vectorLength = -1;
        NewLocalNArray _newLocalNArray;
        NewLocalScalarNArray _newLocalScalarNArray;

        public BlockExpressionBuilder(IList<NArray> independentVariables,
            INArrayFactory factory)
        {
            // for fast look-up
            _independentVariables = new HashSet<NArray>(independentVariables);

            _newLocalNArray = (index, length) => factory.NewLocalNArray(index, length, 1, 0);
            _newLocalScalarNArray = factory.NewLocalScalarNArray; 
        }

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

        public void AddAssign(NArray result, NArray a)
        {
            _operations.Add(
                Expression.Assign(GetParameter(result), GetParameter(a)));
        }

        public void AddBinaryElementWiseOperation(NArray a, NArray b,
            NArray result, ExpressionType operation)
        {
            AddBinaryElementWiseOperation(
                GetParameter(a), GetParameter(b), GetParameter(result), operation);
        }

        public void AddBinaryElementWiseOperation(VectorParameterExpression a, VectorParameterExpression b,
            VectorParameterExpression result, ExpressionType operation)
        {
            _operations.Add(
                Expression.Assign(result,
                Expression.MakeBinary(operation, a, b))
                );
        }

        public void AddUnaryElementWiseOperation(NArray a,
            NArray result, UnaryElementWiseOperations operation)
        {
            _operations.Add(
                Expression.Assign(GetParameter(result),
                ExpressionExtended.MakeUnary(operation, GetParameter(a)))
                );
        }

        public void AddScaleInverseOperation(NArray a, double scale, NArray result)
        {
            _operations.Add(
                Expression.Assign(GetParameter(result),
                ExpressionExtended.ScaleInverse(GetParameter(a), scale))
                );
        }

        public void AddScaleOffsetOperation(NArray a, double scale, double offset, NArray result)
        {
            _operations.Add(
                Expression.Assign(GetParameter(result),
                ExpressionExtended.ScaleOffset(GetParameter(a), scale, offset))
                );
        }

        /// <summary>
        /// Adds a product expression efficiently; assumes that scalar independent variables can be combined
        /// i.e. that any differentiation has already occurred
        /// </summary>
        public VectorParameterExpression AddProductExpression(Expression a,
            Expression b)
        {
            var paramA = a as ReferencingVectorParameterExpression;
            var paramB = b as ReferencingVectorParameterExpression;
            if (paramA == null || paramB == null) throw new NotImplementedException();
            if (paramA.IsScalar && paramB.IsScalar)
            {
                return new ConstantExpression(
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
                return AddLocalAssignment(Expression.MakeBinary(ExpressionType.Multiply, paramA, paramB));
            }
        }

        /// <summary>
        /// Adds a sum expression efficiently; assumes that scalar independent variables can be combined
        /// i.e. that any differentiation has already occurred
        /// </summary>
        public VectorParameterExpression AddAdditionExpression(Expression a, 
            Expression b)
        {
            var paramA = a as ReferencingVectorParameterExpression;
            var paramB = b as ReferencingVectorParameterExpression;
            if (paramA == null || paramB == null) throw new NotImplementedException();
            if (paramA.IsScalar && paramB.IsScalar)
            {
                return new ConstantExpression(
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
                return AddLocalAssignment(Expression.MakeBinary(ExpressionType.Add, paramA, paramB));
            }
        }

        /// <summary>
        /// Adds a division and negation expression efficiently; assumes that scalar independent variables can be combined
        /// i.e. that any differentiation has already occurred
        /// </summary>
        public Expression AddNegateDivideExpression(VectorParameterExpression a, VectorParameterExpression b)
        {
            var paramA = a as ReferencingVectorParameterExpression;
            var paramB = b as ReferencingVectorParameterExpression;
            if (paramA.IsScalar && paramB.IsScalar)
            {
                return new ConstantExpression(
                    -paramA.ScalarValue /
                    paramB.ScalarValue);
            }
            var newLocal = AddLocalAssignment(Expression.Divide(a, b));
            return AddLocalAssignment(new ScaleOffsetExpression(newLocal, -1, 0));
        }

        public Expression AddInverseExpression(VectorParameterExpression a)
        {
            var paramA = a as ReferencingVectorParameterExpression;
            if (paramA.IsScalar)
            {
                return new ConstantExpression(1 / paramA.ScalarValue);
            }
            return AddLocalAssignment(new ScaleInverseExpression(a, 1));
        }

        public Expression AddHalfInverseSquareRootExpression(VectorParameterExpression a)
        {
            var paramA = a as ReferencingVectorParameterExpression;
            if (paramA.IsScalar)
            {
                return new ConstantExpression(0.5 / Math.Sqrt(paramA.ScalarValue));
            }
            var newLocal = AddLocalAssignment(new ScaleInverseExpression(a, 1));
            return AddLocalAssignment(new ScaleOffsetExpression(newLocal, 0.5, 0));
        }

        private static double _gaussianScaling = 1.0 / Math.Sqrt(2.0 * Math.PI);

        public Expression AddGaussian(VectorParameterExpression a)
        {
            var paramA = a as ReferencingVectorParameterExpression;
            if (paramA.IsScalar)
            {
                return new ConstantExpression(_gaussianScaling * Math.Exp(-paramA.ScalarValue * paramA.ScalarValue / 2));
            }
            var newLocal1 = AddLocalAssignment(Expression.MakeBinary(ExpressionType.Multiply, paramA, paramA));
            var newLocal2 = AddLocalAssignment(new ScaleOffsetExpression(newLocal1, -0.5, 0));
            var newLocal3 = AddLocalAssignment(new UnaryMathsExpression(UnaryElementWiseOperations.Exp, newLocal2));
            return AddLocalAssignment(new ScaleOffsetExpression(newLocal3, _gaussianScaling, 0));
        }

        public VectorParameterExpression AddLocalAssignment(Expression rhs)
        {
            LocalNArray dummy;
            var lhs = CreateLocalOfLength(_vectorLength, out dummy);
            _operations.Add(
                Expression.Assign(lhs, rhs));
            return lhs;
        }

        public VectorParameterExpression AddLocalAssignment(double scalarValue, Expression rhs)
        {
            LocalNArray dummy;
            var lhs = CreateScalarLocal(scalarValue, out dummy);
            _operations.Add(
                Expression.Assign(lhs, rhs));
            return lhs;
        }

        public LocalNArray CreateScalarLocal(double value, bool isIndependentVariable)
        {
            LocalNArray array;
            CreateScalarLocal(value, out array);
            if (isIndependentVariable) _independentVariables.Add(array);
            return array;
        }

        public LocalNArray CreateLocal()
        {
            return CreateLocalOfLength(_vectorLength);
        }

        private LocalNArray CreateLocalOfLength(int length)
        {
            LocalNArray array;
            CreateLocalOfLength(length, out array);
            return array;
        }

        private ReferencingVectorParameterExpression CreateLocalOfLength(int length, out LocalNArray array)
        {
            var index = NextLocalIndex();
            array = _newLocalNArray(index, length);
            var parameter = new ReferencingVectorParameterExpression(array, ParameterType.Local, index);
            _localParameters.Add(parameter);
            return parameter;
        }

        private ReferencingVectorParameterExpression CreateScalarLocal(double value, out LocalNArray array)
        {
            var index = NextLocalIndex();
            array = _newLocalScalarNArray(index, value);
            var parameter = new ReferencingVectorParameterExpression(array[0], ParameterType.Local, index);
            _localParameters.Add(parameter);
            return parameter;
        }

        private int NextLocalIndex()
        {
            return _localParameters.Count == 0 ? 0 : _localParameters.Last().Index + 1; // we assume only that list is in increasing order: we can thereby renumber
        }

        public VectorParameterExpression GetParameter(NArray array)
        {
            var local = array as LocalNArray;
            if (local != null)
            {
                return _localParameters[local.Index] as VectorParameterExpression;
            }
            else
            {
                // if the array is a scalar and not an independent variable of any derivative calculation, we do not care where it came from; do not 
                // add to argument list
                if (array.IsScalar && !IsIndependentVariable(array)) return new ConstantExpression(array[0]);
                VectorParameterExpression argument;
                // is this fast enough?
                if (!_argumentLookup.TryGetValue(array, out argument))
                {
                    argument = array.IsScalar ? new ReferencingVectorParameterExpression(array[0], ParameterType.Argument, _argumentLookup.Count)
                        : new ReferencingVectorParameterExpression(array, ParameterType.Argument, _argumentLookup.Count);
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

        public bool IsIndependentVariable(NArray array)
        {
            return _independentVariables.Contains(array);
        }

        #region Simplification

        public BinaryExpression SimplifyOperation(BinaryExpression assignmentExpression)
        {
            // if operation is a scalar, nothing to do (result is already calculated)
            if ((assignmentExpression.Left as ReferencingVectorParameterExpression).IsScalar) return null;

            // if one side of operation is scalar, can be simplified:
            if (assignmentExpression.Right is BinaryExpression)
            {
                var binaryExpression = assignmentExpression.Right as BinaryExpression;
                var left = binaryExpression.Left as ReferencingVectorParameterExpression;
                var right = binaryExpression.Right as ReferencingVectorParameterExpression;
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
                                return Expression.Assign(assignmentExpression.Left, new ScaleOffsetExpression(right, 1, left.ScalarValue));
                            case (ExpressionType.Subtract):
                                return Expression.Assign(assignmentExpression.Left, new ScaleOffsetExpression(right, -1, left.ScalarValue));
                            case (ExpressionType.Multiply):
                                return Expression.Assign(assignmentExpression.Left, new ScaleOffsetExpression(right, left.ScalarValue, 0));
                            case (ExpressionType.Divide):
                                return Expression.Assign(assignmentExpression.Left, new ScaleInverseExpression(right, left.ScalarValue));
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
                            return Expression.Assign(assignmentExpression.Left, new ScaleOffsetExpression(left, 1, right.ScalarValue));
                        case (ExpressionType.Subtract):
                            return Expression.Assign(assignmentExpression.Left, new ScaleOffsetExpression(left, 1, -right.ScalarValue));
                        case (ExpressionType.Multiply):
                            return Expression.Assign(assignmentExpression.Left, new ScaleOffsetExpression(left, right.ScalarValue, 0));
                        case (ExpressionType.Divide):
                            return Expression.Assign(assignmentExpression.Left, new ScaleOffsetExpression(left, 1 / right.ScalarValue, 0));
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            else if (assignmentExpression.Right is UnaryMathsExpression)
            {
                var unaryMathsExpression = assignmentExpression.Right as UnaryMathsExpression;
                var operand = unaryMathsExpression.Operand as ReferencingVectorParameterExpression;
                if (operand.IsScalar) 
                {
                    throw new NotImplementedException(); // should not happen
                }
            }
            return assignmentExpression;
        }

        public static NArray GetArray(Expression expression)
        {
            return (expression as ReferencingVectorParameterExpression).Array;
        }

        private static bool IsScalar(Expression parameter)
        {
            return (parameter as ReferencingVectorParameterExpression).Array.IsScalar;
        }

        private static double ScalarValue(Expression parameter)
        {
            return (parameter as ReferencingVectorParameterExpression).Array[0];
        }

        #endregion
    }
}
