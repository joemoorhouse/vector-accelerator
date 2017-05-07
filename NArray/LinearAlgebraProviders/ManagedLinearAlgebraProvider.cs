using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NArray.DeferredExecution.Expressions;
using NArray.Interfaces;

namespace NArray.LinearAlgebraProviders
{
    class ManagedLinearAlgebraProvider : ILinearAlgebraProvider
    {
        public void BinaryElementWiseOperation(INArrayStorage left, INArrayStorage right, ExpressionType operation, INArrayStorage result)
        {
            VectorVectorOperation vectorVectorOperation = null;
            switch (operation)
            {
                case ExpressionType.Add: vectorVectorOperation = Add; break;
                case ExpressionType.Subtract: vectorVectorOperation = Subtract; break;
                case ExpressionType.Multiply: vectorVectorOperation = Multiply; break;
                case ExpressionType.Divide: vectorVectorOperation = Divide; break;
            }
            VectorVectorOperation(left, right, result, vectorVectorOperation);
        }

        public void UnaryElementWiseOperation(INArrayStorage operand,
            INArrayStorage result, UnaryElementWiseOperations operation)
        {
            if (operation == UnaryElementWiseOperations.Negate)
            {
                ScaleOffset(operand, -1, 0, result);
                return;
            }
            VectorOperation vectorVectorOperation = null;
            switch (operation)
            {
                case UnaryElementWiseOperations.CumulativeNormal: vectorVectorOperation = CumulativeNormal; break;
                case UnaryElementWiseOperations.Exp: vectorVectorOperation = Exp; break;
                case UnaryElementWiseOperations.Inverse: vectorVectorOperation = Inverse; break;
                case UnaryElementWiseOperations.InverseCumulativeNormal: vectorVectorOperation = InverseCumulativeNormal; break;
                case UnaryElementWiseOperations.InverseSquareRoot: vectorVectorOperation = InverseSquareRoot; break;
                case UnaryElementWiseOperations.Log: vectorVectorOperation = Log; break;
                case UnaryElementWiseOperations.SquareRoot: vectorVectorOperation = SquareRoot; break;
            }
            VectorOperation(operand, result, vectorVectorOperation);
        }

        public void ScaleInverse(INArrayStorage operand, double scale, INArrayStorage result)
        {
            VectorOperation(operand, result, IntelVectorMathLibraryWrapper.Inverse);
            IntelVectorMathLibraryWrapper.ConstantAddMultiply(operand.Data, 0, scale, 0, result.Data, 0, result.TotalSize);
        }

        public void ScaleOffset(INArrayStorage operand, double scale, double offset, INArrayStorage result)
        {
            IntelVectorMathLibraryWrapper.ConstantAddMultiply(operand.Data, 0, scale, offset, result.Data, 0, result.TotalSize);
        }

        public void RelativeElementWiseOperation(INArrayStorage left, INArrayStorage right, 
            ExpressionType operation, INArrayBoolStorage result)
        {
            double[] leftArray = left.Data;
            double[] rightArray = right.Data;
            bool[] resultArray = result.Data;
            int leftStart = left.DataStart;
            int rightStart = right.DataStart;
            int resultStart = result.DataStart;

            switch (operation)
            {
                case ExpressionType.LessThan:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] < rightArray[rightStart + i];
                    break;

                case ExpressionType.LessThanOrEqual:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] <= rightArray[rightStart + i];
                    break;

                case ExpressionType.Equal:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] == rightArray[rightStart + i];
                    break;

                case ExpressionType.NotEqual:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] != rightArray[rightStart + i];
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] >= rightArray[rightStart + i];
                    break;

                case ExpressionType.GreaterThan:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] > rightArray[rightStart + i];
                    break;
            }
        }

        public void RelativeElementWiseOperation(INArrayStorage left, double right, ExpressionType operation, INArrayBoolStorage result)
        {
            double[] leftArray = left.Data;
            bool[] resultArray = result.Data;
            int leftStart = left.DataStart;
            int resultStart = result.DataStart;

            switch (operation)
            {
                case ExpressionType.LessThan:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] < right;
                    break;

                case ExpressionType.LessThanOrEqual:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] <= right;
                    break;

                case ExpressionType.Equal:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] == right;
                    break;

                case ExpressionType.NotEqual:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] != right;
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] >= right;
                    break;

                case ExpressionType.GreaterThan:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] > right;
                    break;
            }
        }

        private static void VectorVectorOperation(INArrayStorage left, INArrayStorage right, INArrayStorage result, VectorVectorOperation operation)
        {
            operation(left.Data, 0, right.Data, 0, result.Data, 0, result.TotalSize);
        }

        private static void VectorOperation(INArrayStorage operand, INArrayStorage result, VectorOperation operation)
        {
            operation(operand.Data, 0, result.Data, 0, result.TotalSize);
        }

        private static void Add(double[] left, int leftStartIndex,
            double[] right, int rightStartIndex,
            double[] result, int resultStartIndex,
            int length)
        {
            int leftIndex = leftStartIndex;
            int rightIndex = rightStartIndex;
            int resultIndex = resultStartIndex;
            for (int i = 0; i < length; ++i, leftIndex++, rightIndex++, resultIndex++)
            {
                result[resultIndex] = left[leftIndex] + right[rightIndex];
            }
        }

        //private static void AddMatrix(double[] left, double[] right, int rows, int columns, 
        //    bool leftTranspose, bool rightTranspose, bool rowMajor)
        //{
        //    int leftStride1 = rowMajor ? columns : 1;
        //    int rightStride1 = rowMajor ? 1 : rows;
                     
        //}

        private static void GetStrides(int rows, int columns, bool isTranspose, bool isRowMajor, out int stride1, out int stride2)
        {
            if (!isTranspose)
            {
                stride1 = isRowMajor ? columns : 1;
                stride2 = isRowMajor ? 1 : rows;
            }
            else
            {
                stride1 = isRowMajor ? 1 : rows;
                stride2 = isRowMajor ? columns : 1;
            }
        }

        private static void AddMatrix(double[] left, int leftStride1, int leftStride2,
            double[] right, int rightStride1, int rightStride2,
            int count1, int count2,
            double[] result)
        {
            int index = 0;
            for (int i = 0; i < count1; ++i)
            {
                for (int j = 0; j < count2; ++j)
                {
                    result[index] = left[leftStride1 * i + leftStride2 * j] + right[leftStride1 * i + leftStride2 * j];
                    index++;
                }
            }
        }

        private int MatrixIndex(int x, int y, int stride, bool columnMajor)
        {
            return columnMajor ? x + y * stride : y + x * stride;
        }

        public static void Subtract(double[] left, int leftStartIndex,
            double[] right, int rightStartIndex,
            double[] result, int resultStartIndex,
            int length)
        {
            int leftIndex = leftStartIndex;
            int rightIndex = rightStartIndex;
            int resultIndex = resultStartIndex;
            for (int i = 0; i < length; ++i, leftIndex++, rightIndex++, resultIndex++)
            {
                result[resultIndex] = left[leftIndex] - right[rightIndex];
            }
        }

        public static void Multiply(double[] left, int leftStartIndex,
            double[] right, int rightStartIndex,
            double[] result, int resultStartIndex,
            int length)
        {
            int leftIndex = leftStartIndex;
            int rightIndex = rightStartIndex;
            int resultIndex = resultStartIndex;
            for (int i = 0; i < length; ++i, leftIndex++, rightIndex++, resultIndex++)
            {
                result[resultIndex] = left[leftIndex] * right[rightIndex];
            }
        }

        public static void Divide(double[] left, int leftStartIndex,
            double[] right, int rightStartIndex,
            double[] result, int resultStartIndex,
            int length)
        {
            int leftIndex = leftStartIndex;
            int rightIndex = rightStartIndex;
            int resultIndex = resultStartIndex;
            for (int i = 0; i < length; ++i, leftIndex++, rightIndex++, resultIndex++)
            {
                result[resultIndex] = left[leftIndex] / right[rightIndex];
            }
        }

        public static void CumulativeNormal(double[] operand, int operandStartIndex,
            double[] result, int resultStartIndex,
            int length)
        {
            throw new NotImplementedException();
        }

        public static void Exp(double[] operand, int operandStartIndex,
            double[] result, int resultStartIndex,
            int length)
        {
            int operandIndex = operandStartIndex;
            int resultIndex = resultStartIndex;
            for (int i = 0; i < length; ++i, operandIndex++, resultIndex++)
            {
                result[resultIndex] = Math.Exp(operand[operandIndex]);
            }
        }

        public static void Inverse(double[] operand, int operandStartIndex,
            double[] result, int resultStartIndex,
            int length)
        {
            int operandIndex = operandStartIndex;
            int resultIndex = resultStartIndex;
            for (int i = 0; i < length; ++i, operandIndex++, resultIndex++)
            {
                result[resultIndex] = 1.0 / operand[operandIndex];
            }
        }

        public static void InverseCumulativeNormal(double[] operand, int operandStartIndex,
            double[] result, int resultStartIndex,
            int length)
        {
            throw new NotImplementedException();
        }

        public static void InverseSquareRoot(double[] operand, int operandStartIndex,
            double[] result, int resultStartIndex,
            int length)
        {
            int operandIndex = operandStartIndex;
            int resultIndex = resultStartIndex;
            for (int i = 0; i < length; ++i, operandIndex++, resultIndex++)
            {
                result[resultIndex] = 1.0 / Math.Sqrt(operand[operandIndex]);
            }
        }

        public static void Log(double[] operand, int operandStartIndex,
            double[] result, int resultStartIndex,
            int length)
        {
            int operandIndex = operandStartIndex;
            int resultIndex = resultStartIndex;
            for (int i = 0; i < length; ++i, operandIndex++, resultIndex++)
            {
                result[resultIndex] = Math.Log(operand[operandIndex]);
            }
        }

        public static void SquareRoot(double[] operand, int operandStartIndex,
            double[] result, int resultStartIndex,
            int length)
        {
            int operandIndex = operandStartIndex;
            int resultIndex = resultStartIndex;
            for (int i = 0; i < length; ++i, operandIndex++, resultIndex++)
            {
                result[resultIndex] = Math.Sqrt(operand[operandIndex]);
            }
        }
    }
}
