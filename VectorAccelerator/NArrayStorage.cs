using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator
{
    public enum StorageScheme { ColumnMajor, RowMajor };

    public enum MatrixRegion { All, UpperTriangle, LowerTriangle };

    /// <summary>
    /// Storage is column-major, i.e. an enumerator that passes through the contiguous memory block
    /// goes down the first column first, then the second column and so on.
    /// This is because cuBlas and MKL have (mostly) a column-major convention
    /// The storage is not specified in this base-class, but we assume it will be a contiguous memory block.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class NArrayStorage<T>
    {
        int _rows;
        int _columns;
        int _length;

        /// <summary>
        /// Number of rows
        /// </summary>
        public int Rows { get { return _rows; } } 

        /// <summary>
        /// Number of columns
        /// </summary>
        public int Columns { get { return _columns; } }

        /// <summary>
        /// Length of vector, or total number of elements in matrix
        /// </summary>
        public int Length { get { return _length; } } 

        public NArrayStorage() { }
        
        public NArrayStorage(int length)
        {
            _rows = _length = length;
            _columns = 1; // i.e. we create a column matrix
        }

        public bool IsScalar { get { return (Length == 1); } }
        public bool IsVector { get { return (Length > 1) && (Rows == 1 || Columns == 1); } }
        public bool IsMatrix { get { return Rows > 1 && Columns > 1; } }

        public NArrayStorage(int rowCount, int columnCount)
        {
            _length = rowCount * columnCount;
            _rows = rowCount;
            _columns = columnCount;
        }

        //internal abstract T this[int index] { get; set; }

        //public abstract T First();

        //internal abstract bool Matches(NArrayStorage<T> other);

        /// <summary>
        /// Returns a slice of the storage as a vector that references the same underlying data.
        /// No new storage is created.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        //internal abstract NArrayStorage<T> SliceAsReference(int startIndex, int length);

        /// <summary>
        /// Returns a column as a vector that references the same underlying data.
        /// No new storage is created.
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        //public abstract NArrayStorage<T> ColumnAsReference(int columnIndex);

        /// <summary>
        /// Creates a copy of a sub-matrix, allocating new storage.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="sourceRowIndex"></param>
        /// <param name="targetRowIndex"></param>
        /// <param name="rowCount"></param>
        /// <param name="sourceColumnIndex"></param>
        /// <param name="targetColumnIndex"></param>
        /// <param name="columnCount"></param>
        //internal abstract void CopySubMatrixTo(NArrayStorage<T> target,
        //    int sourceRowIndex, int targetRowIndex, int rowCount,
        //    int sourceColumnIndex, int targetColumnIndex, int columnCount);

        /// <summary>
        /// Creates a transpose of the storage, allocating new storage.
        /// </summary>
        /// <returns></returns>
        //internal abstract NArrayStorage<T> Transpose();

        //internal abstract NArrayStorage<T> Diagonal(int rowCount, int columnCount);

        /// <summary>
        /// Crates a copy of the storage in new memory.
        /// </summary>
        /// <param name="region">THe region to clone</param>
        /// <returns></returns>
        //internal abstract NArrayStorage<T> Clone(MatrixRegion region = MatrixRegion.All);
    }
}
