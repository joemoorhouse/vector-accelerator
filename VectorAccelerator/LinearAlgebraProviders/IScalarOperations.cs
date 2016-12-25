namespace VectorAccelerator.LinearAlgebraProviders
{
    public interface IScalarOperations<T>
    {
        NArray<T> NewScalarNArray(T scalarValue);

        void Convert(int value, out T result);

        void Negate(T value, out T result);

        T Add(T left, T right);

        T Subtract(T left, T right);

        T Multiply(T left, T right);

        T Divide(T left, T right);
    }
}
