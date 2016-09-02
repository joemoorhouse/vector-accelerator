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
    public class IntelMKLLinearAlgebraProvider : LinearAlgebraProvider
    {
        #region ElementWise

        public override void BinaryElementWiseOperation(NArray<double> a, NArray<double> b,
            NArray<double> result, ExpressionType operation)
        {
            return;
            VectorVectorOperation vectorVectorOperation = null;
            switch (operation)
            {
                case ExpressionType.Add: vectorVectorOperation = IntelMathKernelLibrary.Add; break;
                case ExpressionType.Subtract: vectorVectorOperation = IntelMathKernelLibrary.Subtract; break;
                case ExpressionType.Multiply: vectorVectorOperation = IntelMathKernelLibrary.Multiply; break;
                case ExpressionType.Divide: vectorVectorOperation = IntelMathKernelLibrary.Divide; break;
            }
            VectorVectorOperation(a, b, result, vectorVectorOperation);
        }

        public override void BinaryElementWiseOperation(NArray<int> a, NArray<int> b,
            NArray<int> result, ExpressionType operation)
        {
            int[] aArray, bArray, resultArray;
            int aStart, bStart, resultStart;
            GetArray(a, out aArray, out aStart);
            GetArray(b, out bArray, out bStart);
            GetArray(result, out resultArray, out resultStart);

            if (operation == ExpressionType.Add)
            {
                for (int i = 0; i < result.Length; ++i) resultArray[resultStart + i] = aArray[aStart + i] + bArray[bStart + i];
            }
            else if (operation == ExpressionType.Subtract)
            {
                for (int i = 0; i < result.Length; ++i) resultArray[resultStart + i] = aArray[aStart + i] - bArray[bStart + i];
            }
        }

        public override void UnaryElementWiseOperation(NArray<double> a,
            NArray<double> result, UnaryElementWiseOperation operation)
        {
            return;
            if (operation == VectorAccelerator.UnaryElementWiseOperation.Negate)
            {
                ScaleOffset(a, -1, 0, result);
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
            VectorOperation(a, result, vectorVectorOperation);
        }

        public override void UnaryElementWiseOperation(NArray<int> a,
            NArray<int> result, UnaryElementWiseOperation operation)
        {
            throw new NotImplementedException();
        }

        public override void ScaleInverse(NArray<double> a, double scale, NArray<double> result)
        {
            return;
            double[] aArray, resultArray;
            int aStart, resultStart;
            GetArray(a, out aArray, out aStart);
            GetArray(result, out resultArray, out resultStart);
            VectorOperation(a, result, IntelMathKernelLibrary.Inverse);
            IntelMathKernelLibrary.ConstantAddMultiply(resultArray, resultStart, scale, 0, resultArray, resultStart, result.Length);
        }

        public override void ScaleInverse(NArray<int> a, int scale, NArray<int> result)
        {
            throw new NotImplementedException();
        }

        public override void ScaleOffset(NArray<double> a, double scale, double offset, NArray<double> result)
        {
            return;
            double[] aArray, resultArray;
            int aStart, resultStart;
            GetArray(a, out aArray, out aStart);
            GetArray(result, out resultArray, out resultStart);
            IntelMathKernelLibrary.ConstantAddMultiply(aArray, aStart, scale, offset, resultArray, resultStart, result.Length);
        }

        public override void ScaleOffset(NArray<int> a, int scale, int offset, NArray<int> result)
        {
            int[] aArray, resultArray;
            int aStart, resultStart;
            GetArray(a, out aArray, out aStart);
            GetArray(result, out resultArray, out resultStart);
            for (int i = 0; i < result.Length; ++i) resultArray[resultStart + i] = aArray[aStart + i] * scale + offset;
        }

        public override void RelativeOperation(NArray<double> a, NArray<double> b, NArrayBool result, RelativeOperator op)
        {
            double[] aArray, bArray;
            bool[] resultArray;
            int aStart, bStart, resultStart;
            GetArray(a, out aArray, out aStart);
            GetArray(b, out bArray, out bStart);
            GetArray(result, out resultArray, out resultStart);

            switch (op)
            {
                case RelativeOperator.LessThan:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = aArray[aStart + i] < bArray[bStart + i];
                    break;

                case RelativeOperator.LessThanEquals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = aArray[aStart + i] <= bArray[bStart + i];
                    break;

                case RelativeOperator.Equals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = aArray[aStart + i] == bArray[bStart + i];
                    break;

                case RelativeOperator.NotEquals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = aArray[aStart + i] != bArray[bStart + i];
                    break;

                case RelativeOperator.GreaterThanEquals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = aArray[aStart + i] >= bArray[bStart + i];
                    break;

                case RelativeOperator.GreaterThan:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = aArray[aStart + i] > bArray[bStart + i];
                    break;
            }
        }

        public override void RelativeOperation(NArray<double> a, double b, NArrayBool result, RelativeOperator op)
        {
            double[] aArray;
            bool[] resultArray;
            int aStart, resultStart;
            GetArray(a, out aArray, out aStart);
            GetArray(result, out resultArray, out resultStart);

            switch (op)
            {
                case RelativeOperator.LessThan:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = aArray[aStart + i] < b;
                    break;

                case RelativeOperator.LessThanEquals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = aArray[aStart + i] <= b;
                    break;

                case RelativeOperator.Equals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = aArray[aStart + i] == b;
                    break;

                case RelativeOperator.NotEquals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = aArray[aStart + i] != b;
                    break;

                case RelativeOperator.GreaterThanEquals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = aArray[aStart + i] >= b;
                    break;

                case RelativeOperator.GreaterThan:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = aArray[aStart + i] > b;
                    break;
            }
        }

        public override void RelativeOperation(NArray<int> a, NArray<int> b, NArrayBool result, RelativeOperator op)
        {
            int[] aArray, bArray;
            bool[] resultArray;
            int aStart, bStart, resultStart;
            GetArray(a, out aArray, out aStart);
            GetArray(b, out bArray, out bStart);
            GetArray(result, out resultArray, out resultStart);

            switch (op)
            {
                case RelativeOperator.LessThan:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = aArray[aStart + i] < bArray[bStart + i];
                    break;

                case RelativeOperator.LessThanEquals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = aArray[aStart + i] <= bArray[bStart + i];
                    break;

                case RelativeOperator.Equals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = aArray[aStart + i] == bArray[bStart + i];
                    break;

                case RelativeOperator.NotEquals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = aArray[aStart + i] != bArray[bStart + i];
                    break;

                case RelativeOperator.GreaterThanEquals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = aArray[aStart + i] >= bArray[bStart + i];
                    break;

                case RelativeOperator.GreaterThan:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = aArray[aStart + i] > bArray[bStart + i];
                    break;
            }
        }

        public override void RelativeOperation(NArray<int> a, int b, NArrayBool result, RelativeOperator op)
        {
            int[] aArray;
            bool[] resultArray;
            int aStart, resultStart;
            GetArray(a, out aArray, out aStart);
            GetArray(result, out resultArray, out resultStart);

            switch (op)
            {
                case RelativeOperator.LessThan:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = aArray[aStart + i] < b;
                    break;

                case RelativeOperator.LessThanEquals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = aArray[aStart + i] <= b;
                    break;

                case RelativeOperator.Equals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = aArray[aStart + i] == b;
                    break;

                case RelativeOperator.NotEquals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = aArray[aStart + i] != b;
                    break;

                case RelativeOperator.GreaterThanEquals:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = aArray[aStart + i] >= b;
                    break;

                case RelativeOperator.GreaterThan:
                    for (int i = 0; i < resultArray.Length; ++i)
                        resultArray[resultStart + i] = aArray[aStart + i] > b;
                    break;
            }
        }

        public override void LogicalOperation(NArrayBool a, NArrayBool b, NArrayBool result, LogicalBinaryElementWiseOperation operation)
        {
            bool[] aArray, bArray, resultArray;
            int aStart, bStart, resultStart;
            GetArray(a, out aArray, out aStart);
            GetArray(b, out bArray, out bStart);
            GetArray(result, out resultArray, out resultStart);
            if (operation == LogicalBinaryElementWiseOperation.And)
            {
                for (int i = 0; i < result.Length; ++i) resultArray[resultStart + i] = aArray[aStart + i] && bArray[bStart + i];
            }
            else if (operation == LogicalBinaryElementWiseOperation.Or)
            {
                for (int i = 0; i < result.Length; ++i) resultArray[resultStart + i] = aArray[aStart + i] || bArray[bStart + i];
            }
        }

        public override void RightShift(NArray<int> a, int shift, NArray<int> result)
        {
            int[] aArray, resultArray;
            int aStart, resultStart;
            GetArray(a, out aArray, out aStart);
            GetArray(result, out resultArray, out resultStart);
            for (int i = 0; i < result.Length; ++i) resultArray[resultStart + i] = aArray[aStart + i] >> shift;
        }

        public override void LeftShift(NArray<int> a, int shift, NArray<int> result)
        {
            int[] aArray, resultArray;
            int aStart, resultStart;
            GetArray(a, out aArray, out aStart);
            GetArray(result, out resultArray, out resultStart);
            for (int i = 0; i < result.Length; ++i) resultArray[resultStart + i] = aArray[aStart + i] >> shift;
        }

        #endregion

        #region Non-ElementWise

        public override void Assign<T>(NArray<T> a, NArrayBool condition, NArray<T> result)
        {
            T[] aArray, resultArray;
            bool[] cArray;
            int aStart, cStart, resultStart;
            GetArray(a, out aArray, out aStart);
            GetArray(condition, out cArray, out cStart);
            GetArray(result, out resultArray, out resultStart);
            if (a.IsScalar)
            {
                for (int i = 0; i < result.Length; ++i)
                {
                    if (cArray[cStart + i]) resultArray[resultStart + i] = aArray[aStart];
                }
            }
            else
            {
                for (int i = 0; i < result.Length; ++i)
                {
                    if (cArray[cStart + i]) resultArray[resultStart + i] = aArray[aStart + i];
                }
            }
        }

        public override void FillRandom(ContinuousDistribution distribution, NArray values)
        {
            var stream = distribution.RandomNumberStream.InnerStream as IntelMKLRandomNumberStream;
            if (stream == null)
            {
                throw new ArgumentException("distribution must use MKL random generator", "distribution");
            }
            double[] aArray;
            int aStart;
            GetArray(values, out aArray, out aStart);
            // we use the random stream as appropriate
            IntelMathKernelLibraryRandom.FillNormals(aArray, stream, aStart, values.Length);
        }

        public override double Dot(NArray a, NArray b)
        {
            Assertions.AssertVectorsOfEqualLength(a, b);
            return IntelMathKernelLibrary.Dot(GetManagedStorage<double>(a), GetManagedStorage<double>(b));
        }

        public override double Sum(NArray a)
        {
            var storage = GetManagedStorage<double>(a);
            double sum = 0;
            // no performance optimisation on CPU when doing immediate execution (no MKL 'sum')
            for (int i = storage.ArrayStart; i < storage.Length; ++i) sum += storage.Array[i];
            return sum;
        }

        public override void MatrixMultiply(NArray a, NArray b, NArray c)
        {
            IntelMathKernelLibrary.MatrixMultiply(GetManagedStorage<double>(a), GetManagedStorage<double>(b),
                GetManagedStorage<double>(c));
        }

        public override void CholeskyDecomposition(NArray a)
        {
            bool positiveSemiDefinite = true;
            IntelMathKernelLibrary.CholeskyDecomposition(GetManagedStorage<double>(a), out positiveSemiDefinite);
        }

        public override void EigenvalueDecomposition(NArray a, NArray eigenvectors, NArray eigenvalues)
        {
            IntelMathKernelLibrary.EigenvalueDecomposition(GetManagedStorage<double>(a), 
                GetManagedStorage<double>(eigenvectors), GetManagedStorage<double>(eigenvalues));
        }

        public override void SortInPlace(NArray a)
        {
            IntelMathKernelLibrary.SortInPlace(GetManagedStorage<double>(a));
        }

        public override IDisposable CreateRandomNumberStream(RandomNumberGeneratorType type, int seed)
        {
            return new IntelMKLRandomNumberStream(type, seed);
        }

        public override void Index<T>(NArray<T> a, NArrayInt indices, NArray<T> result)
        {
            T[] aArray, resultArray;
            int[] indicesArray;
            int aStart, indicesStart, resultStart;
            GetArray(a, out aArray, out aStart);
            GetArray(indices, out indicesArray, out indicesStart);
            GetArray(result, out resultArray, out resultStart);
            for (int i = 0; i < result.Length; ++i) resultArray[resultStart + i] = aArray[aStart + indicesArray[indicesStart + i]];
        }

        #endregion

        #region Private Methods

        private void VectorVectorOperation(NArray<double> a, NArray<double> b, NArray<double> result, VectorVectorOperation operation)
        {
            double[] aArray, bArray, resultArray;
            int aStart, bStart, resultStart;
            GetArray(a, out aArray, out aStart);
            GetArray(b, out bArray, out bStart);
            GetArray(result, out resultArray, out resultStart);
            operation(aArray, aStart, bArray, bStart, resultArray, resultStart, result.Length);
        }

        private void VectorOperation(NArray<double> a, NArray<double> result, VectorOperation operation)
        {
            double[] aArray, resultArray;
            int aStart, resultStart;
            GetArray(a, out aArray, out aStart);
            GetArray(result, out resultArray, out resultStart);
            operation(aArray, aStart, resultArray, resultStart, result.Length);
        }

        private void GetArray<T>(NArray<T> vector, out T[] array, out int startIndex)
        {
            var managedStorage = vector.Storage as ManagedStorage<T>;
            if (managedStorage == null)
            {
                throw new ArgumentException("storage not managed or mismatching.");
            }
            array = managedStorage.Array;
            startIndex = managedStorage.ArrayStart;
        }

        private ManagedStorage<T> GetManagedStorage<T>(NArray<T> vector)
        {
            return vector.Storage as ManagedStorage<T>;
        }

        #endregion
    }

    public delegate void VectorVectorOperation(double[] a, int aStartIndex,
        double[] b, int bStartIndex,
        double[] y, int yStartIndex,
        int length);

    public delegate void VectorOperation(double[] a, int aStartIndex,
        double[] y, int yStartIndex,
        int length);
}
