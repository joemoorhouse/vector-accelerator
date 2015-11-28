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
        int _chunksCount;
        int _length;
        T[][] _storage;

        public int ChunkLength
        {
            get { return _chunkLength; }
        }

        public ChunkyStorage(int chunksCount, int chunkLength) : base(chunksCount * chunkLength)
        {
            _chunkLength = chunkLength;
            _chunksCount = chunksCount;
            _length = chunksCount * chunkLength;
            _storage = new T[_chunksCount][];
        }

        public void SetChunk(int chunkIndex, T[] chunkStorage)
        {
            if (chunkStorage.Length != _chunkLength)
                throw new ArgumentException("length of array mismatch", "chunkStorage");
            _storage[chunkIndex] = chunkStorage;
        }

        internal NArrayStorage<T> Slice(int chunkIndex)
        {
            return new ManagedStorage<T>(_storage[chunkIndex], 0, _chunkLength);
        }

        internal override NArrayStorage<T> SliceAsReference(int startIndex, int length)
        {
            throw new NotImplementedException();
        }

        internal override NArrayStorage<T> ColumnAsReference(int columnIndex)
        {
            throw new NotImplementedException();
        }

        public T[] GetChunk(int chunkIndex)
        {
            return _storage[chunkIndex];
        }

        internal override T this[int index]
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

        internal override bool Matches(NArrayStorage<T> other)
        {
            throw new NotImplementedException();
        }

        internal override T First()
        {
            throw new NotImplementedException();
        }

        internal override void CopySubMatrixTo(NArrayStorage<T> target, int sourceRowIndex, int targetRowIndex, int rowCount, int sourceColumnIndex, int targetColumnIndex, int columnCount)
        {
            throw new NotImplementedException();
        }

        internal override NArrayStorage<T> Transpose()
        {
            throw new NotImplementedException();
        }

        internal override NArrayStorage<T> Diagonal(int rowCount, int columnCount)
        {
            throw new NotImplementedException();
        }

        internal override NArrayStorage<T> Clone(MatrixRegion region = MatrixRegion.All)
        {
            throw new NotImplementedException();
        }
    }
}
