using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorAccelerator
{
    public interface INArrayStorage<T> 
    {
        int Rows { get; }
        int Columns { get; }
        int Length { get; }

        bool IsScalar { get; }
        bool IsVector { get; }
        bool IsMatrix { get; }

        T this[int index] { get; set; }

        T this[int row, int column] { get; set; }

        T First();

        void CopySubMatrixTo(INArrayStorage<T> target,
            int sourceRowIndex, int targetRowIndex, int rowCount,
            int sourceColumnIndex, int targetColumnIndex, int columnCount);

        /// <summary>
        /// Returns a slice of the storage as a vector that references the same underlying data.
        /// No new storage is created.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        INArrayStorage<T> SliceAsReference(int startIndex, int length);

        INArrayStorage<T> ColumnAsReference(int columnIndex);

        INArrayStorage<T> Clone(MatrixRegion region);

        INArrayStorage<T> Transpose();

        INArrayStorage<T> Diagonal(int row, int column);
    }
}
