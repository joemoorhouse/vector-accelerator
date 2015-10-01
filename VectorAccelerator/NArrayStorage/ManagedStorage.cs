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

        public ManagedStorage(T value) : base(1)
        {
            CreateStorage(value);
        }

        public ManagedStorage(int length) : base(length) 
        {
            CreateStorage(Length);
        }

        public ManagedStorage(int rowCount, int columnCount) : base(rowCount * columnCount)
        {
            CreateStorage(Length);
        }

        public ManagedStorage(T[] array) : base(array.Length) 
        {
            CreateStorage(array, 0, array.Length);
        }

        public ManagedStorage(T[] array, int startIndex, int length) : base(length)
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

        private void CreateStorage(int length)
        {
            _storage = new T[length];
        }

        private void CreateStorage(T value)
        {
            _storage = new T[1];
            _storage[0] = value;
        }

        private void CreateStorage(T[] array, int startIndex, int length)
        {
            _storage = array;
            _storageStart = startIndex;
        }

        public override T First()
        {
            return _storage[0];
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

    public class ManagedStorageCreator<T> : IStorageCreator<T>
    {
        public NArrayStorage<T> NewStorage(int rowCount, int columnCount)
        {
            return new ManagedStorage<T>(rowCount, columnCount);
        }

        public NArrayStorage<T> NewStorage(T[] array)
        {
            return new ManagedStorage<T>(array);
        }

        public NArrayStorage<T> NewStorage(T value)
        {
            return new ManagedStorage<T>(value);
        }
    }

    public class NullStorageCreator<T> : IStorageCreator<T>
    {
        public NArrayStorage<T> NewStorage(int rowCount, int columnCount)
        {
            return null;
        }

        public NArrayStorage<T> NewStorage(T[] array)
        {
            return null;
        }

        public NArrayStorage<T> NewStorage(T value)
        {
            return null;
        }
    }
}
