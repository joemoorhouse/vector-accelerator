using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator.NArrayStorage
{
    public class ManagedStorage<T> : NArrayStorage<T>
    {
        T[] _storage;

        public ManagedStorage(T value) : base(value) { }

        public ManagedStorage(int length) : base(length) { }

        public ManagedStorage(T[] array) : base(array) { }

        public override T this[int index]
        {
            get { return _storage[index]; }
            set { _storage[index] = value; }
        }

        public T[] Array
        {
            get { return _storage; }
        }

        protected override void CreateStorage(int length)
        {
            _storage = new T[length];
        }

        protected override void CreateStorage(T[] array)
        {
            _storage = array;
        }

        public override T First()
        {
            return _storage[0];
        }

        public override int Length
        {
            get { return _storage.Length; }
        }

        public override bool Matches(NArrayStorage<T> other)
        {
            var managedOther = other as ManagedStorage<double>;
            if (managedOther == null) return false;
            return managedOther.Length == Length;
        }
    }
}
