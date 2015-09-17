using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator.NArrayStorage
{
    /// <summary>
    /// NArray where the storage is split into chunks. This is typically useful for cache efficiency.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ChunkyStorage<T> : NArrayStorage<T>
    {
        int _chunkLength;
        int _length;
        T[][] _storage;

        public int ChunkLength
        {
            get { return _chunkLength; }
        }

        public ChunkyStorage(int chunksCount, int chunkLength)
        {
            _storage = new T[chunksCount][];
            _chunkLength = chunkLength;
            _length = chunksCount * chunkLength;
        }

        public void SetChunk(int chunkIndex, T[] chunkStorage)
        {
            if (chunkStorage.Length != _chunkLength)
                throw new ArgumentException("length of array mismatch", "chunkStorage");
            _storage[chunkIndex] = chunkStorage;
        }

        public NArrayStorage<T> Slice(int chunkIndex)
        {
            return new ManagedStorage<T>(_storage[chunkIndex], 0, _chunkLength);
        }

        public override NArrayStorage<T> Slice(int startIndex, int length)
        {
            throw new NotImplementedException();
        }

        public T[] GetChunk(int chunkIndex)
        {
            return _storage[chunkIndex];
        }

        protected override void CreateStorage(int length)
        {
            throw new NotImplementedException();
        }

        protected override void CreateStorage(T[] array, int startIndex, int length)
        {
            throw new NotImplementedException();
        }

        public override int Length
        {
            get { return _length; }
        }

        public override T this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override bool Matches(NArrayStorage<T> other)
        {
            throw new NotImplementedException();
        }

        public override T First()
        {
            throw new NotImplementedException();
        }
    }
}
