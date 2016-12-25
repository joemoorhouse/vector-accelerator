using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VectorAccelerator;
using VectorAccelerator.Distributions;
using VectorAccelerator.NArrayStorage;
using VectorAccelerator.DeferredExecution.Expressions;
//using System.Linq.Expressions;

namespace VectorAccelerator.LinearAlgebraProviders
{
    public partial class IntelMKLLinearAlgebraProvider 
    {
        public void BinaryElementWiseOperation(NArray<double> left, NArray<double> right,
           NArray<double> result, ExpressionType operation)
        {
            VectorVectorOperation vectorVectorOperation = null;
            switch (operation)
            {
                case ExpressionType.Add: vectorVectorOperation = IntelMathKernelLibrary.Add; break;
                case ExpressionType.Subtract: vectorVectorOperation = IntelMathKernelLibrary.Subtract; break;
                case ExpressionType.Multiply: vectorVectorOperation = IntelMathKernelLibrary.Multiply; break;
                case ExpressionType.Divide: vectorVectorOperation = IntelMathKernelLibrary.Divide; break;
            }
            VectorVectorOperation(left, right, result, vectorVectorOperation);
        }

        public void UnaryElementWiseOperation(NArray<double> left,
            NArray<double> result, UnaryElementWiseOperation operation)
        {
            if (operation == VectorAccelerator.UnaryElementWiseOperation.Negate)
            {
                ScaleOffset(left, -1, 0, result);
                return;
            }
            VectorOperation vectorVectorOperation = null;
            switch (operation)
            {
                case VectorAccelerator.UnaryElementWiseOperation.CumulativeNormal: vectorVectorOperation = IntelMathKernelLibrary.CumulativeNormal; break;
                case VectorAccelerator.UnaryElementWiseOperation.Exp: vectorVectorOperation = IntelMathKernelLibrary.Exp; break;
                case VectorAccelerator.UnaryElementWiseOperation.InverseCumulativeNormal: vectorVectorOperation = IntelMathKernelLibrary.InverseCumulativeNormal; break;
                case VectorAccelerator.UnaryElementWiseOperation.InverseSquareRoot: vectorVectorOperation = IntelMathKernelLibrary.InverseSquareRoot; break;
                case VectorAccelerator.UnaryElementWiseOperation.Inverse: vectorVectorOperation = IntelMathKernelLibrary.Inverse; break;
                case VectorAccelerator.UnaryElementWiseOperation.Log: vectorVectorOperation = IntelMathKernelLibrary.Log; break;
                case VectorAccelerator.UnaryElementWiseOperation.SquareRoot: vectorVectorOperation = IntelMathKernelLibrary.SquareRoot; break;
            }
            VectorOperation(left, result, vectorVectorOperation);
        }

        public void ScaleInverse(NArray<double> left, double scale, NArray<double> result)
        {
            double[] leftArray, resultArray;
            int leftStart, resultStart;
            GetArray(left, out leftArray, out leftStart);
            GetArray(result, out resultArray, out resultStart);
            VectorOperation(left, result, IntelMathKernelLibrary.Inverse);
            IntelMathKernelLibrary.ConstantAddMultiply(resultArray, resultStart, scale, 0, resultArray, resultStart, result.Length);
        }

        public void ScaleOffset(NArray<double> left, double scale, double offset, NArray<double> result)
        {
            double[] leftArray, resultArray;
            int leftStart, resultStart;
            GetArray(left, out leftArray, out leftStart);
            GetArray(result, out resultArray, out resultStart);
            IntelMathKernelLibrary.ConstantAddMultiply(leftArray, leftStart, scale, offset, resultArray, resultStart, result.Length);
        }

        public void RelativeOperation(NArray<double> left, NArray<double> right, NArrayBool result, RelativeOperation op)
        {
            double[] leftArray, rightArray;
            bool[] resultArray;
            int leftStart, rightStart, resultStart;
            GetArray(left, out leftArray, out leftStart);
            GetArray(right, out rightArray, out rightStart);
            GetArray(result, out resultArray, out resultStart);

            switch (op)
            {
                case VectorAccelerator.RelativeOperation.LessThan:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] < rightArray[rightStart + i];
                    break;

                case VectorAccelerator.RelativeOperation.LessThanEquals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] <= rightArray[rightStart + i];
                    break;

                case VectorAccelerator.RelativeOperation.Equals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] == rightArray[rightStart + i];
                    break;

                case VectorAccelerator.RelativeOperation.NotEquals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] != rightArray[rightStart + i];
                    break;

                case VectorAccelerator.RelativeOperation.GreaterThanEquals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] >= rightArray[rightStart + i];
                    break;

                case VectorAccelerator.RelativeOperation.GreaterThan:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] > rightArray[rightStart + i];
                    break;
            }
        }

        public void RelativeOperation(NArray<double> left, double right, NArrayBool result, RelativeOperation op)
        {
            double[] leftArray;
            bool[] resultArray;
            int leftStart, resultStart;
            GetArray(left, out leftArray, out leftStart);
            GetArray(result, out resultArray, out resultStart);

            switch (op)
            {
                case VectorAccelerator.RelativeOperation.LessThan:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] < right;
                    break;

                case VectorAccelerator.RelativeOperation.LessThanEquals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] <= right;
                    break;

                case VectorAccelerator.RelativeOperation.Equals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] == right;
                    break;

                case VectorAccelerator.RelativeOperation.NotEquals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] != right;
                    break;

                case VectorAccelerator.RelativeOperation.GreaterThanEquals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] >= right;
                    break;

                case VectorAccelerator.RelativeOperation.GreaterThan:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] > right;
                    break;
            }
        }
    }

    public partial class IntelMKLLinearAlgebraProvider : LinearAlgebraProvider, ILinearAlgebraProvider
    {
        #region ElementWise
    
        public void BinaryElementWiseOperation(NArray<int> left, NArray<int> right,
            NArray<int> result, ExpressionType operation)
        {
            int[] leftArray, rightArray, resultArray;
            int leftStart, rightStart, resultStart;
            GetArray(left, out leftArray, out leftStart);
            GetArray(right, out rightArray, out rightStart);
            GetArray(result, out resultArray, out resultStart);

            if (operation == ExpressionType.Add)
            {
                for (int i = 0; i < result.Length; ++i) resultArray[resultStart + i] = leftArray[leftStart + i] + rightArray[rightStart + i];
            }
            else if (operation == ExpressionType.Subtract)
            {
                for (int i = 0; i < result.Length; ++i) resultArray[resultStart + i] = leftArray[leftStart + i] - rightArray[rightStart + i];
            }
        }

        public void UnaryElementWiseOperation(NArray<int> left,
            NArray<int> result, UnaryElementWiseOperation operation)
        {
            throw new NotImplementedException();
        }

        public void ScaleInverse(NArray<int> left, int scale, NArray<int> result)
        {
            throw new NotImplementedException();
        }

        public void ScaleOffset(NArray<int> left, int scale, int offset, NArray<int> result)
        {
            int[] leftArray, resultArray;
            int leftStart, resultStart;
            GetArray(left, out leftArray, out leftStart);
            GetArray(result, out resultArray, out resultStart);
            for (int i = 0; i < result.Length; ++i) resultArray[resultStart + i] = leftArray[leftStart + i] * scale + offset;
        }

        public void RelativeOperation(NArray<int> left, NArray<int> right, NArrayBool result, RelativeOperation op)
        {
            int[] leftArray, rightArray;
            bool[] resultArray;
            int leftStart, rightStart, resultStart;
            GetArray(left, out leftArray, out leftStart);
            GetArray(right, out rightArray, out rightStart);
            GetArray(result, out resultArray, out resultStart);

            switch (op)
            {
                case VectorAccelerator.RelativeOperation.LessThan:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] < rightArray[rightStart + i];
                    break;

                case VectorAccelerator.RelativeOperation.LessThanEquals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] <= rightArray[rightStart + i];
                    break;

                case VectorAccelerator.RelativeOperation.Equals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] == rightArray[rightStart + i];
                    break;

                case VectorAccelerator.RelativeOperation.NotEquals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] != rightArray[rightStart + i];
                    break;

                case VectorAccelerator.RelativeOperation.GreaterThanEquals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] >= rightArray[rightStart + i];
                    break;

                case VectorAccelerator.RelativeOperation.GreaterThan:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] > rightArray[rightStart + i];
                    break;
            }
        }

        public void RelativeOperation(NArray<int> left, int right, NArrayBool result, RelativeOperation op)
        {
            int[] leftArray;
            bool[] resultArray;
            int leftStart, resultStart;
            GetArray(left, out leftArray, out leftStart);
            GetArray(result, out resultArray, out resultStart);

            switch (op)
            {
                case VectorAccelerator.RelativeOperation.LessThan:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] < right;
                    break;

                case VectorAccelerator.RelativeOperation.LessThanEquals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] <= right;
                    break;

                case VectorAccelerator.RelativeOperation.Equals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] == right;
                    break;

                case VectorAccelerator.RelativeOperation.NotEquals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] != right;
                    break;

                case VectorAccelerator.RelativeOperation.GreaterThanEquals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] >= right;
                    break;

                case VectorAccelerator.RelativeOperation.GreaterThan:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = leftArray[leftStart + i] > right;
                    break;
            }
        }

        public void LogicalOperation(NArrayBool left, NArrayBool right, NArrayBool result, LogicalBinaryElementWiseOperation operation)
        {
            bool[] leftArray, rightArray, resultArray;
            int leftStart, rightStart, resultStart;
            GetArray(left, out leftArray, out leftStart);
            GetArray(right, out rightArray, out rightStart);
            GetArray(result, out resultArray, out resultStart);
            if (operation == LogicalBinaryElementWiseOperation.And)
            {
                for (int i = 0; i < result.Length; ++i) resultArray[resultStart + i] = leftArray[leftStart + i] && rightArray[rightStart + i];
            }
            else if (operation == LogicalBinaryElementWiseOperation.Or)
            {
                for (int i = 0; i < result.Length; ++i) resultArray[resultStart + i] = leftArray[leftStart + i] || rightArray[rightStart + i];
            }
        }

        public void RightShift(NArray<int> left, int shift, NArray<int> result)
        {
            int[] leftArray, resultArray;
            int leftStart, resultStart;
            GetArray(left, out leftArray, out leftStart);
            GetArray(result, out resultArray, out resultStart);
            for (int i = 0; i < result.Length; ++i) resultArray[resultStart + i] = leftArray[leftStart + i] >> shift;
        }

        public void LeftShift(NArray<int> left, int shift, NArray<int> result)
        {
            int[] leftArray, resultArray;
            int leftStart, resultStart;
            GetArray(left, out leftArray, out leftStart);
            GetArray(result, out resultArray, out resultStart);
            for (int i = 0; i < result.Length; ++i) resultArray[resultStart + i] = leftArray[leftStart + i] >> shift;
        }

        #endregion

        #region Non-ElementWise

        public void Assign<T>(NArray<T> left, NArrayBool condition, NArray<T> result)
        {
            T[] leftArray, resultArray;
            bool[] cArray;
            int leftStart, cStart, resultStart;
            GetArray(left, out leftArray, out leftStart);
            GetArray(condition, out cArray, out cStart);
            GetArray(result, out resultArray, out resultStart);
            if (left.IsScalar)
            {
                for (int i = 0; i < result.Length; ++i)
                {
                    if (cArray[cStart + i]) resultArray[resultStart + i] = leftArray[leftStart];
                }
            }
            else
            {
                for (int i = 0; i < result.Length; ++i)
                {
                    if (cArray[cStart + i]) resultArray[resultStart + i] = leftArray[leftStart + i];
                }
            }
        }

        public void FillRandom(ContinuousDistribution distribution, NArray values)
        {
            var stream = distribution.RandomNumberStream.InnerStream as IntelMKLRandomNumberStream;
            if (stream == null)
            {
                throw new ArgumentException("distribution must use MKL random generator", "distribution");
            }
            double[] leftArray;
            int leftStart;
            GetArray(values, out leftArray, out leftStart);
            // we use the random stream as appropriate
            IntelMathKernelLibraryRandom.FillNormals(leftArray, stream, leftStart, values.Length);
        }

        public double Dot(NArray left, NArray right)
        {
            Assertions.AssertVectorsOfEqualLength(left, right);
            return IntelMathKernelLibrary.Dot(GetManagedStorage<double>(left), GetManagedStorage<double>(right));
        }

        public double Sum(NArray left)
        {
            var storage = GetManagedStorage<double>(left);
            double sum = 0;
            // no performance optimisation on CPU when doing immediate execution (no MKL 'sum')
            for (int i = storage.DataStartIndex; i < storage.Length; ++i) sum += storage.Data[i];
            return sum;
        }

        public void MatrixMultiply(NArray left, NArray right, NArray c)
        {
            IntelMathKernelLibrary.MatrixMultiply(GetManagedStorage<double>(left), GetManagedStorage<double>(right),
                GetManagedStorage<double>(c));
        }

        public void CholeskyDecomposition(NArray left)
        {
            bool positiveSemiDefinite = true;
            IntelMathKernelLibrary.CholeskyDecomposition(GetManagedStorage<double>(left), out positiveSemiDefinite);
        }

        public void EigenvalueDecomposition(NArray left, NArray eigenvectors, NArray eigenvalues)
        {
            IntelMathKernelLibrary.EigenvalueDecomposition(GetManagedStorage<double>(left), 
                GetManagedStorage<double>(eigenvectors), GetManagedStorage<double>(eigenvalues));
        }

        public void SortInPlace(NArray left)
        {
            IntelMathKernelLibrary.SortInPlace(GetManagedStorage<double>(left));
        }

        public IDisposable CreateRandomNumberStream(RandomNumberGeneratorType type, int seed)
        {
            return new IntelMKLRandomNumberStream(type, seed);
        }

        public void Index<T>(NArray<T> left, NArrayInt indices, NArray<T> result)
        {
            T[] leftArray, resultArray;
            int[] indicesArray;
            int leftStart, indicesStart, resultStart;
            GetArray(left, out leftArray, out leftStart);
            GetArray(indices, out indicesArray, out indicesStart);
            GetArray(result, out resultArray, out resultStart);
            for (int i = 0; i < result.Length; ++i) resultArray[resultStart + i] = leftArray[leftStart + indicesArray[indicesStart + i]];
        }

        #endregion

        #region Private Methods

        private void VectorVectorOperation(NArray<double> left, NArray<double> right, NArray<double> result, VectorVectorOperation operation)
        {
            double[] leftArray, rightArray, resultArray;
            int leftStart, rightStart, resultStart;
            GetArray(left, out leftArray, out leftStart);
            GetArray(right, out rightArray, out rightStart);
            GetArray(result, out resultArray, out resultStart);
            operation(leftArray, leftStart, rightArray, rightStart, resultArray, resultStart, result.Length);
        }

        private void VectorOperation(NArray<double> left, NArray<double> result, VectorOperation operation)
        {
            double[] leftArray, resultArray;
            int leftStart, resultStart;
            GetArray(left, out leftArray, out leftStart);
            GetArray(result, out resultArray, out resultStart);
            operation(leftArray, leftStart, resultArray, resultStart, result.Length);
        }

        private void GetArray<T>(NArray<T> vector, out T[] array, out int startIndex)
        {
            var managedStorage = vector.Storage as ManagedStorage<T>;
            if (managedStorage == null)
            {
                throw new ArgumentException("storage not managed or mismatching.");
            }
            array = managedStorage.Data;
            startIndex = managedStorage.DataStartIndex;
        }

        private ManagedStorage<T> GetManagedStorage<T>(NArray<T> vector)
        {
            return vector.Storage as ManagedStorage<T>;
        }

        #endregion
    }

    public delegate void VectorVectorOperation(double[] left, int aStartIndex,
        double[] right, int bStartIndex,
        double[] result, int yStartIndex,
        int length);

    public delegate void VectorOperation(double[] left, int aStartIndex,
        double[] result, int yStartIndex,
        int length);
}
