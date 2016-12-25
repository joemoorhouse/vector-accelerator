using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using NArray.Interfaces;
using NArray.DeferredExecution;
using NArray.DeferredExecution.Expressions;

namespace NArray
{
    internal class DeferringNArrayOperationExecutor : INArrayOperationExecutor
    {
        BlockExpressionBuilder _builder;

        Func<double, double, double> Add = (left, right) => left + right;
        Func<double, double, double> Subtract = (left, right) => left - right;
        Func<double, double, double> Multiply = (left, right) => left * right;
        Func<double, double, double> Divide = (left, right) => left / right;

        public BlockExpressionBuilder Builder {  get { return _builder; } }

        public DeferringNArrayOperationExecutor(IList<NArray> independentVariables,
            INArrayFactory factory)
        {
            _builder = new BlockExpressionBuilder(independentVariables, factory);
        }

        public NArray ElementWiseAdd(NArray operand1, NArray operand2)
        {
            return BinaryElementWiseOperation(operand1, operand2, ExpressionType.Add, Add);
        }

        public NArray ElementWiseSubtract(NArray operand1, NArray operand2)
        {
            return BinaryElementWiseOperation(operand1, operand2, ExpressionType.Subtract, Subtract);
        }

        public NArray ElementWiseMultiply(NArray operand1, NArray operand2)
        {
            return BinaryElementWiseOperation(operand1, operand2, ExpressionType.Multiply, Multiply);
        }

        public NArray ElementWiseDivide(NArray operand1, NArray operand2)
        {
            return BinaryElementWiseOperation(operand1, operand2, ExpressionType.Divide, Divide);
        }

        public void InPlaceAdd(NArray result, NArray left, NArray right)
        {
            throw new NotImplementedException();
        }

        public void InPlaceMultiply(NArray result, NArray left, NArray right)
        {
            throw new NotImplementedException();
        }

        public NArray UnaryElementWiseOperation(NArray operand, UnaryElementWiseOperations operation)
        {
            NArray result = null;
            if (operand.IsScalar)
            {
                Func<double, double> scalarOperation = null;
                switch (operation)
                {
                    case UnaryElementWiseOperations.Negate:
                        scalarOperation = op => -op;
                        break;
                    case UnaryElementWiseOperations.Exp:
                        scalarOperation = op => Math.Exp(op);
                        break;
                    case UnaryElementWiseOperations.Log:
                        scalarOperation = op => Math.Log(op);
                        break;
                    default:
                        throw new NotImplementedException();
                }
                if (!IsIndependentVariable(operand))
                {
                    return NewScalarLocal(scalarOperation(operand[0]));
                }
                else
                {
                    result = NewScalarLocal(scalarOperation(operand[0]), true);
                }
            }
            else
            {
                result = NewLocalLike(operand);
            }
            _builder.AddUnaryElementWiseOperation(operand, result, operation);

            return result;
        }

        private NArray BinaryElementWiseOperation(NArray left, NArray right,
            ExpressionType type, Func<double, double, double> scalarOperation)
        {
            NArray result = null;
            if (left.IsScalar && right.IsScalar)
            {
                if (!IsIndependentVariable(left) && !IsIndependentVariable(right))
                {
                    return NewScalarLocal(scalarOperation(left[0], right[0])); // this is a scalar that does not depend on any independent variable: can simply return
                }
                else
                {
                    result = NewScalarLocal(scalarOperation(left[0], right[0]), true);
                }
            }
            else if (left.IsScalar) result = NewLocalLike(right);
            else result = NewLocalLike(left);

            _builder.AddBinaryElementWiseOperation(left, right, result, type);
            return result;
        }

        private LocalNArray NewScalarLocal(double value, bool isIndependentVariable = false)
        {
            return _builder.CreateScalarLocal(value, isIndependentVariable);
        }

        private LocalNArray NewLocalLike(NArray array)
        {
            if (_builder.VectorLength == -1) _builder.VectorLength = array.TotalSize;
            if (array.TotalSize != _builder.VectorLength)
                throw new ArgumentException("length mismatch", "array");

            return _builder.CreateLocal();
        }

        private bool IsIndependentVariable(NArray array)
        {
            return _builder.IsIndependentVariable(array);
        }
    }
}
