﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NArray.DeferredExecution.Expressions;
using NArray.Interfaces;

namespace NArray
{
    internal class ImmediateNArrayOperationExecutor : INArrayOperationExecutor
    {
        Func<double, NArray> _newScalarNArray;
        Func<int, int, double, NArray> _newNArray;
        Func<NArray, NArray> _newNArrayLike;
        INArrayFactory _factory;
        ILinearAlgebraProvider _linearAlgebraProvider;

        public ImmediateNArrayOperationExecutor(INArrayFactory factory,
            ILinearAlgebraProvider linearAlgebraProvider)
        {
            _factory = factory;
            _newScalarNArray = factory.NewScalarNArray; 
            _newNArray = factory.NewNArray;
            _newNArrayLike = factory.NewNArrayLike;
            _linearAlgebraProvider = linearAlgebraProvider;
        }

        public INArrayFactory Factory { get { return _factory; } }

        public ILinearAlgebraProvider LinearAlgebraProvider {  get { return _linearAlgebraProvider; } }

        public NArray NewScalarNArray(double scalarValue)
        {
            return _newScalarNArray(scalarValue);
        }

        public NArray NewNArray(double value, int rows, int columns)
        {
            return _newNArray(rows, columns, value);
        }

        public NArray NewNArrayLike(NArray other)
        {
            return _newNArrayLike(other);
        }

        public NArray ElementWiseAdd(NArray left, NArray right)
        {
            if (left.IsScalar && right.IsScalar) return _newScalarNArray(left[0] + right[0]);

            else if (left.IsScalar) return ScaleOffset(right, 1, left[0]);
            else if (right.IsScalar) return ScaleOffset(left, 1, right[0]);

            else return BinaryElementWiseOperation(left, right, 
                ExpressionType.Add);
        }

        public NArray ElementWiseSubtract(NArray left, NArray right)
        {
            if (left.IsScalar && right.IsScalar) return _newScalarNArray(left[0] - right[0]);

            else if (left.IsScalar) return ScaleOffset(right, -1, left[0]);
            else if (right.IsScalar) return ScaleOffset(left, 1, -right[0]);

            else return BinaryElementWiseOperation(left, right,
                ExpressionType.Subtract);
        }

        public NArray ElementWiseMultiply(NArray left, NArray right)
        {
            if (left.IsScalar && right.IsScalar) return _newScalarNArray(left[0] * right[0]);

            else if (left.IsScalar) return ScaleOffset(right, left[0], 0);
            else if (right.IsScalar) return ScaleOffset(left, right[0], 0);

            else return BinaryElementWiseOperation(left, right,
                ExpressionType.Multiply);
        }

        public NArray ElementWiseDivide(NArray left, NArray right)
        {
            if (left.IsScalar && right.IsScalar) return _newScalarNArray(left[0] / right[0]);

            else if (left.IsScalar) return ScaleInverse(right, left[0]);
            else if (right.IsScalar) return ScaleOffset(left, 1.0 / right[0], 0);

            else return BinaryElementWiseOperation(left, right,
                ExpressionType.Divide);
        }

        public void InPlaceAdd(NArray result, NArray left, NArray right)
        {
            InPlaceBinaryOperation(result.Storage, left.Storage, right.Storage, ExpressionType.Add);
        }

        public void InPlaceMultiply(NArray result, NArray left, NArray right)
        {
            InPlaceBinaryOperation(result.Storage, left.Storage, right.Storage, ExpressionType.Multiply);
        }

        public NArray UnaryElementWiseOperation(NArray operand, UnaryElementWiseOperations operation)
        {
            var result = _newNArrayLike(operand);
            _linearAlgebraProvider.UnaryElementWiseOperation(operand.Storage, result.Storage, operation);
            return result;
        }

        private NArray ScaleOffset(NArray operand, double scale, double offset)
        {
            var result = _newNArrayLike(operand);
            _linearAlgebraProvider.ScaleOffset(operand.Storage, scale, offset, result.Storage);
            return result;
        }

        private NArray ScaleInverse(NArray operand, double scale)
        {
            var result = _newNArrayLike(operand);
            _linearAlgebraProvider.ScaleInverse(operand.Storage, scale, result.Storage);
            return result;
        }

        private NArray BinaryElementWiseOperation(NArray left, NArray right, ExpressionType operation)
        {
            var result = _newNArrayLike(left);
            _linearAlgebraProvider.BinaryElementWiseOperation(left.Storage, right.Storage, operation, result.Storage);
            return result;
        }

        private void InPlaceBinaryOperation(INArrayStorage result, INArrayStorage left, INArrayStorage right, ExpressionType operation)
        {
            _linearAlgebraProvider.BinaryElementWiseOperation(left, right, operation, result);
        }
    }
}
