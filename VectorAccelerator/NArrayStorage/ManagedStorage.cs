using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator.NArrayStorage
{    
    public class ManagedStorage<T> : NArrayStorage<T>, IManagedStorage<T>
    {
        T[] _data;
        int _dataStartIndex = 0;
        int _stride = 1;

        public T[] Data
        {
            get { return _data; }
        }

        public T this[int index]
        {
            get { return _data[index]; }
            set { _data[index] = value; }
        }

        public T this[int row, int column]
        {
            get { return _data[_dataStartIndex + _stride * column + row]; }
            set { _data[_dataStartIndex + _stride * column + row] = value; }
        }

        public int DataStartIndex
        {
            get { return _dataStartIndex; }
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
            _stride = Rows;
            CreateStorage(array);
        }

        public ManagedStorage<T> Clone()
        {
            return new ManagedStorage<T>((T[])_data.Clone(), _dataStartIndex, Length);
        }

        private void CreateStorage(int length)
        {
            _data = new T[length];
        }

        private void CreateStorage(int length, T value)
        {
            _data = new T[length];
            for (int i = 0; i < _data.Length; ++i) _data[i] = value;
        }

        private void CreateStorage(T value)
        {
            _data = new T[1];
            _data[0] = value;
        }

        private void CreateStorage(T[] array, int startIndex, int length)
        {
            _data = array;
            _dataStartIndex = startIndex;
        }

        private void CreateStorage(T[,] array)
        {
            _data = new T[array.Length];
            int index = 0;
            for (int i = 0; i < Rows; ++i)
            {
                for (int j = 0; j < Columns; ++j)
                {
                    _data[index++] = array[i, j];
                }
            }
        }

        public T First()
        {
            return _data[0];
        }

        public bool Matches(NArrayStorage<T> other)
        {
            var managedOther = other as ManagedStorage<double>;
            if (managedOther == null) return false;
            return managedOther.Length == Length;
        }

        public INArrayStorage<T> SliceAsReference(int startIndex, int length)
        {
            return new ManagedStorage<T>(_data, startIndex, length);
        }

        public INArrayStorage<T> ColumnAsReference(int columnIndex)
        {
            return new ManagedStorage<T>(_data, _dataStartIndex + columnIndex * _stride, Rows);
        }

        public void CopySubMatrixTo(INArrayStorage<T> target, 
            int sourceRowIndex, int targetRowIndex, int rowCount,
            int sourceColumnIndex, int targetColumnIndex, int columnCount)
        {
            var targetManaged = target as ManagedStorage<T>;
         
            for (int j = sourceColumnIndex, jj = targetColumnIndex; j < sourceColumnIndex + columnCount; j++, jj++)
            {
                System.Array.Copy(_data, _dataStartIndex + j * _stride + sourceRowIndex, targetManaged._data, 
                    targetManaged._dataStartIndex + jj * targetManaged._stride + targetRowIndex, rowCount);
            }
        }

        public INArrayStorage<T> Transpose()
        {
            AssertIsNotReferenceMatrix(this);
            var transpose = new ManagedStorage<T>(Columns, Rows);
            for (int i = 0; i < Rows; ++i)
            {
                for (int j = 0; j < Columns; ++j)
                {
                    transpose[j, i] = this[i, j];
                }
            }
            return transpose;
        }

        public INArrayStorage<T> Diagonal(int rowCount, int columnCount)
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

        public INArrayStorage<T> Clone(MatrixRegion region = MatrixRegion.All)
        {
            if (Rows != Columns && region != MatrixRegion.All) throw new ArgumentException("only square matrices can be upper or lower triangular");
            var clone = new ManagedStorage<T>(Rows, Columns);
            CopyRegionTo(clone, region);
            return clone;
        }

        private void CopyRegionTo(ManagedStorage<T> target, MatrixRegion region)
        {
            switch (region)
            {
                case MatrixRegion.All:
                    CopySubMatrixTo(target, 0, 0, Rows, 0, 0, Columns);
                    break;
                case MatrixRegion.LowerTriangle:
                    for (int i = 0; i < Rows; ++i)
                    {
                        for (int j = 0; j <= i; ++j)
                        {
                            target[i, j] = this[i, j];
                        }
                    }
                    break;
                case MatrixRegion.UpperTriangle:
                    for (int i = 0; i < Rows; ++i)
                    {
                        for (int j = i; j < Columns; ++j)
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
                System.Array.Copy(_data, _dataStartIndex + sourceColumnIndex, targetManaged._data,
                    targetManaged._dataStartIndex + targetColumnIndex, columnCount);
            }
            else
            {
                for (int j = sourceColumnIndex, jj = targetColumnIndex; j < sourceColumnIndex + columnCount; j++, jj++)
                {
                    targetManaged._data[targetManaged._dataStartIndex + targetColumnIndex * _stride]
                        = _data[_dataStartIndex + sourceColumnIndex * _stride + rowIndex];
                }
            }
        }

        /// <summary>
        /// Asset that this matrix
        /// </summary>
        /// <param name="storage"></param>
        private static void AssertIsNotReferenceMatrix(ManagedStorage<T> storage)
        {
            if (storage._dataStartIndex != 0 || storage._stride != storage.Rows)
                throw Exceptions.OperationNotSupportedForReferenceMatrices();
        }
    }
}
