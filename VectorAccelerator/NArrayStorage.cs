using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator
{
    public abstract class NArrayStorage<T>
    {
        public NArrayStorage() { }
        
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
            CreateStorage(array, 0, array.Length);
        }

        public abstract T this[int index] { get; set; }

        protected abstract void CreateStorage(int length);

        protected abstract void CreateStorage(T[] array, int startIndex, int length);

        public abstract T First();

        public abstract int Length { get; }

        public abstract bool Matches(NArrayStorage<T> other);

        public abstract NArrayStorage<T> Slice(int startIndex, int length);
    }
}
