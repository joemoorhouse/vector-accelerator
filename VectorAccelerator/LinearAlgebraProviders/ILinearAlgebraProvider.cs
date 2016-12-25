namespace VectorAccelerator.LinearAlgebraProviders
{
    public interface ILinearAlgebraProvider<T> : IElementWiseOperations<T>, IScalarOperations<T>  { }

    public interface ILinearAlgebraProvider : ILinearAlgebraProvider<double>, ILinearAlgebraProvider<int>, ILinearMathsOperations { }
}
