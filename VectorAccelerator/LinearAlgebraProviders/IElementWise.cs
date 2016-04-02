using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using VectorAccelerator.Distributions;

namespace VectorAccelerator.LinearAlgebraProviders
{    
    public interface IElementWise<T>
    {
        void BinaryElementWiseOperation(NArray<T> a, NArray<T> b, NArray<T> result, ExpressionType operation);

        void UnaryElementWiseOperation(NArray<T> a, NArray<T> result, UnaryElementWiseOperation operation);

        /// <summary>
        /// Scales and offsets a vector by constant amounts
        /// </summary>
        /// <param name="a">Input vector</param>
        /// <param name="scale">Scaling factor</param>
        /// <param name="offset">Offset</param>
        /// <returns></returns>
        void ScaleOffset(NArray<T> a, T scale, T offset, NArray<T> result);

        void RelativeOperation(NArray<T> a, NArray<T> b, NArrayBool result, RelativeOperator op);

        void RelativeOperation(NArray<T> a, T b, NArrayBool result, RelativeOperator op);

        void Convert(int value, out T result);

        void Negate(T value, out T result);

        NArray<T> NewScalarNArray(T scalarValue);

        T Add(T a, T b);

        T Subtract(T a, T b);

        T Multiply(T a, T b);

        T Divide(T a, T b);
    }
}
