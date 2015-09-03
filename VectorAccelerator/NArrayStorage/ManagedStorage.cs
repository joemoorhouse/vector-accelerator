using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator.NArrayStorage
{
    public class ManagedStorage<T> : NArrayStorage<T>
    {
        T[] _storage;
        int _storageStart = 0;
        int _length;

        public ManagedStorage(T value) : base(value) { }

        public ManagedStorage(int length) : base(length) { }

        public ManagedStorage(T[] array) : base(array) { }

        public ManagedStorage(T[] array, int startIndex, int length)
        {
            CreateStorage(array, startIndex, length);
        }

        public override T this[int index]
        {
            get { return _storage[index]; }
            set { _storage[index] = value; }
        }

        public T[] Array
        {
            get { return _storage; }
        }

        public int ArrayStart
        {
            get { return _storageStart; }
        }

        protected override void CreateStorage(int length)
        {
            _storage = new T[length];
            _length = length;
        }

        protected override void CreateStorage(T[] array, int startIndex, int length)
        {
            _storage = array;
            _storageStart = startIndex;
            _length = length;
        }

        public override T First()
        {
            return _storage[0];
        }

        public override int Length
        {
            get { return _length; }
        }

        public override bool Matches(NArrayStorage<T> other)
        {
            var managedOther = other as ManagedStorage<double>;
            if (managedOther == null) return false;
            return managedOther.Length == Length;
        }

        public override NArrayStorage<T> Slice(int startIndex, int length)
        {
            return new ManagedStorage<T>(_storage, startIndex, length);
        }
    }
}
