using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VectorAccelerator;
using VectorAccelerator.Distributions;
using VectorAccelerator.NArrayStorage;

namespace VectorAccelerator.LinearAlgebraProviders
{
    public class IntelMKLLinearAlgebraProvider : ILinearAlgebraProvider
    {
        public void Assign<T>(NArray<T> a, NArrayBool condition, NArray<T> result)
        {
            T[] aArray, resultArray;
            bool[] cArray;
            int aStart, cStart, resultStart;
            GetArray(a, out aArray, out aStart);
            GetArray(condition, out cArray, out cStart);
            GetArray(result, out resultArray, out resultStart);
            for (int i = 0; i < result.Length; ++i)
            {
                if (cArray[cStart + i]) resultArray[resultStart + i] = aArray[aStart + i];
            }
        }
        
        public void BinaryElementWiseOperation(NArray<double> a, NArray<double> b, 
            NArray<double> result, BinaryElementWiseOperation operation)
        {
            VectorVectorOperation vectorVectorOperation = null;
            switch (operation)
            {
                case VectorAccelerator.BinaryElementWiseOperation.Add: vectorVectorOperation = IntelMathKernelLibrary.Add; break;
                case VectorAccelerator.BinaryElementWiseOperation.Subtract: vectorVectorOperation = IntelMathKernelLibrary.Subtract; break;
                case VectorAccelerator.BinaryElementWiseOperation.Multiply: vectorVectorOperation = IntelMathKernelLibrary.Multiply; break;
                case VectorAccelerator.BinaryElementWiseOperation.Divide: vectorVectorOperation = IntelMathKernelLibrary.Divide; break;
            }
            VectorVectorOperation(a, b, result, vectorVectorOperation);
        }

        public void BinaryElementWiseOperation(NArray<int> a, NArray<int> b,
            NArray<int> result, BinaryElementWiseOperation operation)
        {
            int[] aArray, bArray, resultArray;
            int aStart, bStart, resultStart;
            GetArray(a, out aArray, out aStart);
            GetArray(b, out bArray, out bStart);
            GetArray(result, out resultArray, out resultStart);

            if (operation == VectorAccelerator.BinaryElementWiseOperation.Add)
            {
                for (int i = 0; i < result.Length; ++i) resultArray[resultStart + i] = aArray[aStart + i] + bArray[bStart + i];
            }
            else if (operation == VectorAccelerator.BinaryElementWiseOperation.Subtract)
            {
                for (int i = 0; i < result.Length; ++i) resultArray[resultStart + i] = aArray[aStart + i] - bArray[bStart + i];
            }
        }

        public void LogicalOperation(NArrayBool a, NArrayBool b, NArrayBool result, LogicalBinaryElementWiseOperation operation)
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

        public void UnaryElementWiseOperation(NArray<double> a,
            NArray<double> result, UnaryElementWiseOperation operation)
        {
            VectorOperation vectorVectorOperation = null;
            switch (operation)
            {
                case VectorAccelerator.UnaryElementWiseOperation.CumulativeNormal: vectorVectorOperation = IntelMathKernelLibrary.CumulativeNormal; break;
                case VectorAccelerator.UnaryElementWiseOperation.Exp: vectorVectorOperation = IntelMathKernelLibrary.Exp; break;
                case VectorAccelerator.UnaryElementWiseOperation.Inverse: vectorVectorOperation = IntelMathKernelLibrary.Inverse; break;
                case VectorAccelerator.UnaryElementWiseOperation.InverseCumulativeNormal: vectorVectorOperation = IntelMathKernelLibrary.InverseCumulativeNormal; break;
                case VectorAccelerator.UnaryElementWiseOperation.InverseSquareRoot: vectorVectorOperation = IntelMathKernelLibrary.InverseSquareRoot; break;
                case VectorAccelerator.UnaryElementWiseOperation.Log: vectorVectorOperation = IntelMathKernelLibrary.Log; break;
                case VectorAccelerator.UnaryElementWiseOperation.SquareRoot: vectorVectorOperation = IntelMathKernelLibrary.SquareRoot; break;
            }
            VectorOperation(a, result, vectorVectorOperation);
        }

        public void RelativeOperation(NArray<int> a, int b, NArrayBool result, UnaryElementWiseOperation operation)
        {
            int[] aArray;
            bool[] resultArray;
            int aStart, resultStart;
            GetArray(a, out aArray, out aStart);
            GetArray(result, out resultArray, out resultStart);
            for (int i = 0; i < result.Length; ++i) resultArray[resultStart + i] = aArray[aStart + i] < b;

        }

        public void ElementWiseRelativeOperation(NArray<int> a, NArray<int> b, NArrayBool result, UnaryElementWiseOperation operation)
        {
            throw new NotImplementedException();
        }

        public void Convert(int value, out double result) { result = value; }
        public void Convert(int value, out int result) { result = value; }
        public void Negate(double value, out double result) { result = -value; }
        public void Negate(int value, out int result) { result = -value; }
        public double Add(double a, double b) { return a + b; }
        public int Add(int a, int b) { return a + b; }
        public double Subtract(double a, double b) { return a - b; }
        public int Subtract(int a, int b) { return a - b; }
        public double Multiply(double a, double b) { return a * b; }
        public int Multiply(int a, int b) { return a * b; }
        public double Divide(double a, double b) { return a / b; }
        public int Divide(int a, int b) { return a / b; }

        public NArray<double> NewScalarNArray(double scalarValue) { return new NArray(scalarValue) as NArray<double>; }
        public NArray<int> NewScalarNArray(int scalarValue) { return new NArrayInt(scalarValue) as NArray<int>; }

        public void UnaryElementWiseOperation(NArray<int> a,
            NArray<int> result, UnaryElementWiseOperation operation)
        {
            throw new NotImplementedException();
        }

        public void ScaleOffset(NArray<double> a, double scale, double offset, NArray<double> result)
        {
            double[] aArray, resultArray;
            int aStart, resultStart;
            GetArray(a, out aArray, out aStart);
            GetArray(result, out resultArray, out resultStart);
            IntelMathKernelLibrary.ConstantAddMultiply(aArray, aStart, scale, offset, resultArray, resultStart, result.Length);
        }

        public void ScaleOffset(NArray<int> a, int scale, int offset, NArray<int> result)
        {
            int[] aArray, resultArray;
            int aStart, resultStart;
            GetArray(a, out aArray, out aStart);
            GetArray(result, out resultArray, out resultStart);
            for (int i = 0; i < result.Length; ++i) resultArray[resultStart + i] = aArray[aStart + i] * scale + offset;
        }

        public void RightShift(NArray<int> a, int shift, NArray<int> result)
        {
            int[] aArray, resultArray;
            int aStart, resultStart;
            GetArray(a, out aArray, out aStart);
            GetArray(result, out resultArray, out resultStart);
            for (int i = 0; i < result.Length; ++i) resultArray[resultStart + i] = aArray[aStart + i] >> shift;
        }

        public void LeftShift(NArray<int> a, int shift, NArray<int> result)
        {
            int[] aArray, resultArray;
            int aStart, resultStart;
            GetArray(a, out aArray, out aStart);
            GetArray(result, out resultArray, out resultStart);
            for (int i = 0; i < result.Length; ++i) resultArray[resultStart + i] = aArray[aStart + i] >> shift;
        }

        public void RelativeOperation(NArray<double> a, NArray<double> b, NArrayBool result, RelativeOperator op)
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

        public void RelativeOperation(NArray<double> a, double b, NArrayBool result, RelativeOperator op)
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

        public void RelativeOperation(NArray<int> a, NArray<int> b, NArrayBool result, RelativeOperator op)
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

        public void RelativeOperation(NArray<int> a, int b, NArrayBool result, RelativeOperator op)
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

        public void MatrixMultiply(NArray a, NArray b, NArray c)
        {

        }

        public static NArray<T> CreateNArray<T>(int length)
        {
            if (typeof(T) == typeof(double)) return new NArray(length) as NArray<T>;
            else if (typeof(T) == typeof(int)) return new NArrayInt(length) as NArray<T>;
            else if (typeof(T) == typeof(bool)) return new NArrayBool(length) as NArray<T>;
            else return null;
        }

        public NArray<S> CreateLike<S, T>(NArray<T> a)
        {
            return CreateNArray<S>(a.Length);
        }

        public NArray<S> CreateConstantLike<S, T>(NArray<T> a, S constantValue)
        {
            var newArray = CreateNArray<S>(a.Length);
            S[] array;
            int arrayStart;
            GetArray(newArray, out array, out arrayStart);
            for (int i = 0; i < array.Length; ++i) array[i] = constantValue;
            return newArray;
        }

        public static NArray<T> CreateNArray<T>(NArrayStorage<T> storage)
        {
            if (typeof(T) == typeof(double)) return new NArray(storage as NArrayStorage<double>) as NArray<T>;
            //else if (typeof(T) == typeof(int)) return new NArrayInt(storage as NArrayStorage<int>) as NArray<T>;
            else return null;
        }

        public IDisposable CreateRandomNumberStream(RandomNumberGeneratorType type, int seed)
        {
            return new IntelMKLRandomNumberStream(type, seed);
        }

        public void FillRandom(ContinuousDistribution distribution, NArray values)
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

        public void VectorVectorOperation(NArray<double> a, NArray<double> b, NArray<double> result, VectorVectorOperation operation)
        {
            double[] aArray, bArray, resultArray;
            int aStart, bStart, resultStart;
            GetArray(a, out aArray, out aStart);
            GetArray(b, out bArray, out bStart);
            GetArray(result, out resultArray, out resultStart);
            operation(aArray, aStart, bArray, bStart, resultArray, resultStart, result.Length);
        }

        public void VectorOperation(NArray<double> a, NArray<double> result, VectorOperation operation)
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

        public void Index<T>(NArray<T> a, NArrayInt indices, NArray<T> result)
        {
            T[] aArray, resultArray;
            int[] indicesArray;
            int aStart, indicesStart, resultStart;
            GetArray(a, out aArray, out aStart);
            GetArray(indices, out indicesArray, out indicesStart);
            GetArray(result, out resultArray, out resultStart);
            for (int i = 0; i < result.Length; ++i) resultArray[resultStart + i] = aArray[aStart + indicesArray[indicesStart + i]];
        }
    }

    public delegate void VectorVectorOperation(double[] a, int aStartIndex,
        double[] b, int bStartIndex,
        double[] y, int yStartIndex,
        int length);

    public delegate void VectorOperation(double[] a, int aStartIndex,
        double[] y, int yStartIndex,
        int length);
}
