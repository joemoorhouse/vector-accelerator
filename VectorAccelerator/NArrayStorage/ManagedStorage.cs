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
        int _stride = 1;

        public T[] Array
        {
            get { return _storage; }
        }

        internal override T this[int index]
        {
            get { return _storage[index]; }
            set { _storage[index] = value; }
        }

        internal T this[int row, int column]
        {
            get { return _storage[_storageStart + _stride * column + row]; }
            set { _storage[_storageStart + _stride * column + row] = value; }
        }

        public int ArrayStart
        {
            get { return _storageStart; }
        }

        public ManagedStorage(T value) : base(1)
        {
            CreateStorage(value);
        }

        public ManagedStorage(int length) : base(length) 
        {
            CreateStorage(Length);
        }

        public ManagedStorage(int rowCount, int columnCount) : base(rowCount, columnCount)
        {
            _stride = rowCount;
            CreateStorage(Length);
        }

        public ManagedStorage(int rowCount, int columnCount, T value)
            : base(rowCount, columnCount)
        {
            _stride = rowCount;
            CreateStorage(Length, value);
        }

        public ManagedStorage(T[] array) : base(array.Length) 
        {
            CreateStorage(array, 0, array.Length);
        }

        public ManagedStorage(T[] array, int startIndex, int length) : base(length)
        {
            CreateStorage(array, startIndex, length);
        }

        public ManagedStorage(T[,] array)
            : base(array.GetLength(0), array.GetLength(1))
        {
            _stride = RowCount;
            CreateStorage(array);
        }

        public ManagedStorage<T> Clone()
        {
            return new ManagedStorage<T>((T[])_storage.Clone(), _storageStart, Length);
        }

        private void CreateStorage(int length)
        {
            _storage = new T[length];
        }

        private void CreateStorage(int length, T value)
        {
            _storage = new T[length];
            for (int i = 0; i < _storage.Length; ++i) _storage[i] = value;
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

        private void CreateStorage(T[,] array)
        {
            _storage = new T[array.Length];
            int index = 0;
            for (int i = 0; i < RowCount; ++i)
            {
                for (int j = 0; j < ColumnCount; ++j)
                {
                    _storage[index++] = array[i, j];
                }
            }
        }

        internal override T First()
        {
            return _storage[0];
        }

        internal override bool Matches(NArrayStorage<T> other)
        {
            var managedOther = other as ManagedStorage<double>;
            if (managedOther == null) return false;
            return managedOther.Length == Length;
        }

        internal override NArrayStorage<T> SliceAsReference(int startIndex, int length)
        {
            return new ManagedStorage<T>(_storage, startIndex, length);
        }

        public override NArrayStorage<T> ColumnAsReference(int columnIndex)
        {
            return new ManagedStorage<T>(_storage, _storageStart + columnIndex * _stride, RowCount);
        }

        internal override void CopySubMatrixTo(NArrayStorage<T> target, 
            int sourceRowIndex, int targetRowIndex, int rowCount,
            int sourceColumnIndex, int targetColumnIndex, int columnCount)
        {
            var targetManaged = target as ManagedStorage<T>;
         
            for (int j = sourceColumnIndex, jj = targetColumnIndex; j < sourceColumnIndex + columnCount; j++, jj++)
            {
                System.Array.Copy(_storage, _storageStart + j * _stride + sourceRowIndex, targetManaged._storage, 
                    targetManaged._storageStart + jj * targetManaged._stride + targetRowIndex, rowCount);
            }
        }

        internal override NArrayStorage<T> Transpose()
        {
            AssertIsNotReferenceMatrix(this);
            var transpose = new ManagedStorage<T>(ColumnCount, RowCount);
            for (int i = 0; i < RowCount; ++i)
            {
                for (int j = 0; j < ColumnCount; ++j)
                {
                    transpose[j, i] = this[i, j];
                }
            }
            return transpose;
        }

        internal override NArrayStorage<T> Diagonal(int rowCount, int columnCount)
        {
            int minDimension = Math.Min(rowCount, columnCount);
            if (this.Length != minDimension)
            {
                throw new ArgumentException("diagonalArray must have length equal to the lesser of the row count and column count of matrixToFill");
            }
            var diagonal = new ManagedStorage<T>(rowCount, columnCount);
            for (int i = 0; i < minDimension; ++i)
            {
                diagonal[i, i] = this[i];
            }
            return diagonal;
        }

        internal override NArrayStorage<T> Clone(MatrixRegion region = MatrixRegion.All)
        {
            if (RowCount != ColumnCount && region != MatrixRegion.All) throw new ArgumentException("only square matrices can be upper or lower triangular");
            var clone = new ManagedStorage<T>(RowCount, ColumnCount);
            CopyRegionTo(clone, region);
            return clone;
        }

        private void CopyRegionTo(ManagedStorage<T> target, MatrixRegion region)
        {
            switch (region)
            {
                case MatrixRegion.All:
                    CopySubMatrixTo(target, 0, 0, RowCount, 0, 0, ColumnCount);
                    break;
                case MatrixRegion.LowerTriangle:
                    for (int i = 0; i < RowCount; ++i)
                    {
                        for (int j = 0; j <= i; ++j)
                        {
                            target[i, j] = this[i, j];
                        }
                    }
                    break;
                case MatrixRegion.UpperTriangle:
                    for (int i = 0; i < RowCount; ++i)
                    {
                        for (int j = i; j < ColumnCount; ++j)
                        {
                            target[i, j] = this[i, j];
                        }
                    }
                    break;
                    
            }
        }

        private void CopySubRowTo(NArrayStorage<T> target, 
            int rowIndex, int sourceColumnIndex, int targetColumnIndex, int columnCount)
        {
            AssertIsNotReferenceMatrix(this);
            
            var targetManaged = target as ManagedStorage<T>;

            AssertIsNotReferenceMatrix(targetManaged);

            // optimisation possible where this and target are both vectors
            if (this._stride == 1 && targetManaged._stride == 1)
            {
                System.Array.Copy(_storage, _storageStart + sourceColumnIndex, targetManaged._storage,
                    targetManaged._storageStart + targetColumnIndex, columnCount);
            }
            else
            {
                for (int j = sourceColumnIndex, jj = targetColumnIndex; j < sourceColumnIndex + columnCount; j++, jj++)
                {
                    targetManaged._storage[targetManaged._storageStart + targetColumnIndex * _stride]
                        = _storage[_storageStart + sourceColumnIndex * _stride + rowIndex];
                }
            }
        }

        /// <summary>
        /// Asset that this matrix
        /// </summary>
        /// <param name="storage"></param>
        private static void AssertIsNotReferenceMatrix(ManagedStorage<T> storage)
        {
            if (storage._storageStart != 0 || storage._stride != storage.RowCount)
                throw Exceptions.OperationNotSupportedForReferenceMatrices();
        }
    }
}
