using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator.Distributions
{    
    public class RandomNumberStream : IDisposable
    {
        public readonly RandomNumberGeneratorType Type;
        public readonly int Seed;

        public IDisposable _innerStream;
        public IDisposable InnerStream { get { return _innerStream;  } }

        public RandomNumberStream(RandomNumberGeneratorType type, int seed)
        {
            _innerStream = ExecutionContext.Executor.CreateRandomNumberStream(type, seed);
            Type = type;
            Seed = seed;
        }

        public void Reset()
        {
            _innerStream.Dispose();
            _innerStream = ExecutionContext.Executor.CreateRandomNumberStream(Type, Seed);
        }

        public void Dispose()
        {
            _innerStream.Dispose();
        }
    }
}
