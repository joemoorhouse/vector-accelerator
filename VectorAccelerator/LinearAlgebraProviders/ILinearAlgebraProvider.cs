using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VectorAccelerator.Distributions;

namespace VectorAccelerator.LinearAlgebraProviders
{    
    public interface IElementWise<T>
    {
        void BinaryElementWiseOperation(NArray<T> a, NArray<T> b, NArray<T> result, BinaryElementWiseOperation operation);

        void UnaryElementWiseOperation(NArray<T> a, NArray<T> result, UnaryElementWiseOperation operation);

        void RelativeOperation(NArray<T> a, NArray<T> b, NArrayBool result, RelativeOperator op);

        void RelativeOperation(NArray<T> a, T b, NArrayBool result, RelativeOperator op);

        void Convert(int value, out T result);

        void Negate(T value, out T result);

        NArray<T> NewScalarNArray(T scalarValue);

        T Add(T a, T b);

        T Subtract(T a, T b);

        T Multiply(T a, T b);

        T Divide(T a, T b);

        /// <summary>
        /// Scales and offsets a vector by constant amounts
        /// </summary>
        /// <param name="a">Input vector</param>
        /// <param name="scale">Scaling factor</param>
        /// <param name="offset">Offset</param>
        /// <returns></returns>
        void ScaleOffset(NArray<T> a, T scale, T offset, NArray<T> result);
    }
    
    public interface ILinearAlgebraProvider : IElementWise<double>, IElementWise<int> 
    {
        void Assign<T>(NArray<T> a, NArrayBool condition, NArray<T> result);
        
        void LogicalOperation(NArrayBool a, NArrayBool b, NArrayBool result, LogicalBinaryElementWiseOperation operation);
        
        void LeftShift(NArray<int> a, int shift, NArray<int> result);

        void RightShift(NArray<int> a, int shift, NArray<int> result);

        /// <summary>
        /// Matrix multiply: c = a * b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        void MatrixMultiply(NArray a, NArray b, NArray c);

        NArray<T> CreateLike<T, S>(NArray<S> a);

        NArray<T> CreateConstantLike<T, S>(NArray<S> a, T constantValue);

        IDisposable CreateRandomNumberStream(RandomNumberGeneratorType type, int seed);

        void FillRandom(ContinuousDistribution distribution, NArray values);

        void Index<T>(NArray<T> a, NArrayInt indices, NArray<T> result);

    }
}
