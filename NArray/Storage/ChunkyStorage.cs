using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NArray.Interfaces;

namespace NArray.Storage
{
    /// <summary>
    /// NArray where the storage is split into chunks. This is typically useful for cache efficiency.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class ChunkyStorage : NArrayShape, INArrayStorage
    {
        int _chunkLength;
        int _chunksCount;
        int _length;
        double[][] _storage;

        public int ChunkLength
        {
            get { return _chunkLength; }
        }

        public ChunkyStorage(int chunksCount, int chunkLength) : base(chunksCount * chunkLength, 1)
        {
            _chunkLength = chunkLength;
            _chunksCount = chunksCount;
            _length = chunksCount * chunkLength;
            _storage = new double[_chunksCount][];
        }

        public void SetChunk(int chunkIndex, double[] chunkStorage)
        {
            if (chunkStorage.Length != _chunkLength)
                throw new ArgumentException("length of array mismatch", "chunkStorage");
            _storage[chunkIndex] = chunkStorage;
        }

        public INArrayStorage Slice(int chunkIndex)
        {
            return new SlicedStorage(_storage[chunkIndex], 0, _chunkLength, 1);
        }

        public double[] GetChunk(int chunkIndex)
        {
            return _storage[chunkIndex];
        }

        public double[] Data { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }

        public int DataStart { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }

        public double this[params int[] indices] { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
    }
}
