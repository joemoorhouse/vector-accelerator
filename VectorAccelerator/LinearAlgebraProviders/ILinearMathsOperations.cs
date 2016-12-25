using System;
using VectorAccelerator.Distributions;

namespace VectorAccelerator.LinearAlgebraProviders
{
    public interface ILinearMathsOperations
    {
        void FillRandom(ContinuousDistribution distribution, NArray values);

        double Dot(NArray left, NArray right);

        double Sum(NArray left);

        void MatrixMultiply(NArray left, NArray right, NArray c);

        void CholeskyDecomposition(NArray operand);

        void EigenvalueDecomposition(NArray operand, NArray eigenvectors, NArray eignenvalues);

        void SortInPlace(NArray operand);

        IDisposable CreateRandomNumberStream(RandomNumberGeneratorType type, int seed);

        void Assign<T>(NArray<T> operand, NArrayBool condition, NArray<T> result);

        void Index<T>(NArray<T> operand, NArrayInt indices, NArray<T> result);

        void LogicalOperation(NArrayBool left, NArrayBool right, NArrayBool result, LogicalBinaryElementWiseOperation operation);

        void LeftShift(NArray<int> operand, int shift, NArray<int> result);

        void RightShift(NArray<int> operand, int shift, NArray<int> result);
    }
}
