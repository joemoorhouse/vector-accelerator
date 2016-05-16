using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VectorAccelerator.Distributions;
using System.Linq.Expressions;

namespace VectorAccelerator.LinearAlgebraProviders
{
    public abstract class LinearAlgebraProvider : IElementWise<double>, IElementWise<int>
    {
        #region Implementation

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

        public NArray<double> NewScalarNArray(double scalarValue) { return new NArray(StorageLocation.Host, scalarValue) as NArray<double>; }

        public NArray<int> NewScalarNArray(int scalarValue) { return new NArrayInt(StorageLocation.Host, scalarValue) as NArray<int>; }

        #endregion

        #region Abstract ElementWise

        public abstract void BinaryElementWiseOperation(NArray<double> a, NArray<double> b, NArray<double> result, ExpressionType operation);

        public abstract void BinaryElementWiseOperation(NArray<int> a, NArray<int> b, NArray<int> result, ExpressionType operation);

        public abstract void UnaryElementWiseOperation(NArray<double> a, NArray<double> result, UnaryElementWiseOperation operation);

        public abstract void UnaryElementWiseOperation(NArray<int> a, NArray<int> result, UnaryElementWiseOperation operation);

        public abstract void ScaleInverse(NArray<double> a, double scale, NArray<double> result);

        public abstract void ScaleInverse(NArray<int> a, int scale, NArray<int> result);

        public abstract void ScaleOffset(NArray<double> a, double scale, double offset, NArray<double> result);

        public abstract void ScaleOffset(NArray<int> a, int scale, int offset, NArray<int> result);

        public abstract void RelativeOperation(NArray<double> a, NArray<double> b, NArrayBool result, RelativeOperator op);

        public abstract void RelativeOperation(NArray<int> a, NArray<int> b, NArrayBool result, RelativeOperator op);

        public abstract void RelativeOperation(NArray<double> a, double b, NArrayBool result, RelativeOperator op);

        public abstract void RelativeOperation(NArray<int> a, int b, NArrayBool result, RelativeOperator op);

        public abstract void LogicalOperation(NArrayBool a, NArrayBool b, NArrayBool result, LogicalBinaryElementWiseOperation operation);

        public abstract void LeftShift(NArray<int> a, int shift, NArray<int> result);

        public abstract void RightShift(NArray<int> a, int shift, NArray<int> result);

        #endregion

        #region Abstract Non-ElementWise

        public abstract void Assign<T>(NArray<T> a, NArrayBool condition, NArray<T> result);

        public abstract void FillRandom(ContinuousDistribution distribution, NArray values);

        public abstract double Dot(NArray a, NArray b);

        public abstract double Sum(NArray a);

        public abstract void MatrixMultiply(NArray a, NArray b, NArray c);

        public abstract void CholeskyDecomposition(NArray a);

        public abstract void EigenvalueDecomposition(NArray a, NArray eigenvectors, NArray eignenvalues);

        public abstract void SortInPlace(NArray a);

        public abstract IDisposable CreateRandomNumberStream(RandomNumberGeneratorType type, int seed);

        public abstract void Index<T>(NArray<T> a, NArrayInt indices, NArray<T> result);

        #endregion
    }
}
