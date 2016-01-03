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

        public IDisposable InnerStream { get { return _innerStream;  } }

        public StorageLocation Location { get { return _location; } }

        public RandomNumberStream(StorageLocation location, 
            RandomNumberGeneratorType type = RandomNumberGeneratorType.MRG32K3A, int seed = 111)
        {
            _location = location;
            _innerStream = ExecutionContext.Executor.CreateRandomNumberStream(location, type, seed);
            Type = type;
            Seed = seed;
        }

        public void Reset()
        {
            _innerStream.Dispose();
            _innerStream = ExecutionContext.Executor.CreateRandomNumberStream(_location, Type, Seed);
        }

        public void Dispose()
        {
            _innerStream.Dispose();
        }

        private IDisposable _innerStream;
        private StorageLocation _location;
    }
}
