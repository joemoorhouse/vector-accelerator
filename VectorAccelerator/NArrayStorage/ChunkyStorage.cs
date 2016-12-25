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
    public class ChunkyStorage<T> : NArrayStorage<T>, INArrayStorage<T>
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

        public INArrayStorage<T> Slice(int chunkIndex)
        {
            return new ManagedStorage<T>(_storage[chunkIndex], 0, _chunkLength);
        }

        public INArrayStorage<T> SliceAsReference(int startIndex, int length)
        {
            throw new NotImplementedException();
        }

        public INArrayStorage<T> ColumnAsReference(int columnIndex)
        {
            throw new NotImplementedException();
        }

        public T[] GetChunk(int chunkIndex)
        {
            return _storage[chunkIndex];
        }

        public T this[int index]
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

        public T this[int row, int column]
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

        public bool Matches(NArrayStorage<T> other)
        {
            throw new NotImplementedException();
        }

        public T First()
        {
            throw new NotImplementedException();
        }

        public void CopySubMatrixTo(INArrayStorage<T> target, int sourceRowIndex, int targetRowIndex, int rowCount, int sourceColumnIndex, int targetColumnIndex, int columnCount)
        {
            throw new NotImplementedException();
        }

        public INArrayStorage<T> Transpose()
        {
            throw new NotImplementedException();
        }

        public INArrayStorage<T> Diagonal(int rowCount, int columnCount)
        {
            throw new NotImplementedException();
        }

        public INArrayStorage<T> Clone(MatrixRegion region = MatrixRegion.All)
        {
            throw new NotImplementedException();
        }
    }
}
