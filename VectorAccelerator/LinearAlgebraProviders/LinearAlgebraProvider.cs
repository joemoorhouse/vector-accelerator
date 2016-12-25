namespace VectorAccelerator.LinearAlgebraProviders
{
    public class LinearAlgebraProvider : IScalarOperations<double>, IScalarOperations<int>
    {
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
    }
}
