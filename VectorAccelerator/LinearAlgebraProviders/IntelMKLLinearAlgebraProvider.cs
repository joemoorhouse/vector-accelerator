using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VectorAccelerator.Distributions;
using VectorAccelerator.NArrayStorage;

namespace VectorAccelerator.LinearAlgebraProviders
{
    public class IntelMKLLinearAlgebraProvider : ILinearAlgebraProvider
    {
        public void Add(NArray a, NArray b, NArray result)
        {
            VectorVectorOperation(a, b, result, IntelMathKernelLibrary.Add);
        }

        public void Subtract(NArray a, NArray b, NArray result)
        {
            VectorVectorOperation(a, b, result, IntelMathKernelLibrary.Subtract);
        }

        public void Multiply(NArray a, NArray b, NArray result)
        {
            VectorVectorOperation(a, b, result, IntelMathKernelLibrary.Multiply);
        }

        public void Divide(NArray a, NArray b, NArray result)
        {
            VectorVectorOperation(a, b, result, IntelMathKernelLibrary.Divide);
        }

        public void Inverse(NArray a, NArray result)
        {
            VectorOperation(a, result, IntelMathKernelLibrary.Inverse);
        }

        public void Exp(NArray a, NArray result)
        {
            VectorOperation(a, result, IntelMathKernelLibrary.Exp);
        }

        public void Log(NArray a, NArray result)
        {
            VectorOperation(a, result, IntelMathKernelLibrary.Log);
        }

        public void SquareRoot(NArray a, NArray result)
        {
            VectorOperation(a, result, IntelMathKernelLibrary.SquareRoot);
        }

        public void InverseSquareRoot(NArray a, NArray result)
        {
            VectorOperation(a, result, IntelMathKernelLibrary.InverseSquareRoot);
        }

        public void CumulativeNormal(NArray a, NArray result)
        {
            VectorOperation(a, result, IntelMathKernelLibrary.CumulativeNormal);
        }

        public void InverseCumulativeNormal(NArray a, NArray result)
        {
            VectorOperation(a, result, IntelMathKernelLibrary.InverseCumulativeNormal);
        }

        public void ScaleOffset(NArray a, double scale, double offset, NArray result)
        {
            double[] aArray, resultArray;
            int aStart, resultStart;
            GetArray(a, out aArray, out aStart);
            GetArray(result, out resultArray, out resultStart);
            IntelMathKernelLibrary.ConstantAddMultiply(aArray, aStart, scale, offset, resultArray, resultStart, result.Length);
        }

        public void MatrixMultiply(NArray a, NArray b, NArray c)
        {

        }

        public NArray CreateLike(NArray a)
        {
            return NArray.CreateLike(a);
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

        public void VectorVectorOperation(NArray a, NArray b, NArray result, VectorVectorOperation operation)
        {
            double[] aArray, bArray, resultArray;
            int aStart, bStart, resultStart;
            GetArray(a, out aArray, out aStart);
            GetArray(b, out bArray, out bStart);
            GetArray(result, out resultArray, out resultStart);
            operation(aArray, aStart, bArray, bStart, resultArray, resultStart, result.Length);
        }

        public void VectorOperation(NArray a, NArray result, VectorOperation operation)
        {
            double[] aArray, resultArray;
            int aStart, resultStart;
            GetArray(a, out aArray, out aStart);
            GetArray(result, out resultArray, out resultStart);
            operation(aArray, aStart, resultArray, resultStart, result.Length);
        }

        private void GetArray(NArray vector, out double[] array, out int startIndex)
        {
            var managedStorage = vector.Storage as ManagedStorage<double>;
            if (managedStorage == null)
            {
                throw new ArgumentException("storage not managed or mismatching.");
            }
            array = managedStorage.Array;
            startIndex = managedStorage.ArrayStart;
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
