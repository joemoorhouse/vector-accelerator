using VectorAccelerator.DeferredExecution.Expressions;

namespace VectorAccelerator.LinearAlgebraProviders
{
    public interface IElementWiseOperations<T>
    {
        void BinaryElementWiseOperation(NArray<T> left, NArray<T> right, NArray<T> result, ExpressionType operation);

        void UnaryElementWiseOperation(NArray<T> operand, NArray<T> result, UnaryElementWiseOperation operation);

        void ScaleInverse(NArray<T> operand, T scale, NArray<T> result);

        void ScaleOffset(NArray<T> operand, T scale, T offset, NArray<T> result);

        void RelativeOperation(NArray<T> left, NArray<T> right, NArrayBool result, RelativeOperation operation);

        void RelativeOperation(NArray<T> left, T right, NArrayBool result, RelativeOperation operation);
    }
}
