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
        T[][] _storage;

        public int ChunkLength
        {
            get { return _chunkLength; }
        }

        public ChunkyStorage(int chunksCount, int chunkLength)
        {
            _storage = new T[chunksCount][];
            _chunkLength = chunkLength;
        }

        public void SetChunk(int chunkIndex, T[] chunkStorage)
        {
            if (chunkStorage.Length != _chunkLength)
                throw new ArgumentException("length of array mismatch", "chunkStorage");
            _storage[chunkIndex] = chunkStorage;
        }

        public T[] GetChunk(int chunkIndex)
        {
            return _storage[chunkIndex];
        }

        protected override void CreateStorage(int length)
        {
            throw new NotImplementedException();
        }

        protected override void CreateStorage(T[] array)
        {
            throw new NotImplementedException();
        }

        public override int Length
        {
            get { throw new NotImplementedException(); }
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
