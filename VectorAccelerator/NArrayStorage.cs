using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator
{
    public abstract class NArrayStorage<T>
    {
        public NArrayStorage()
        {
            throw new NotImplementedException();
        }
        
        public NArrayStorage(int length)
        {
            CreateStorage(length);
        }

        public NArrayStorage(T value)
        {
            CreateStorage(1);
            this[0] = value;
        }

        public NArrayStorage(T[] array)
        {
            CreateStorage(array);
        }

        public abstract T this[int index] { get; set; }

        protected abstract void CreateStorage(int length);

        protected abstract void CreateStorage(T[] array);

        public abstract T First();

        public abstract int Length { get; }

        public abstract bool Matches(NArrayStorage<T> other);
    }
}
