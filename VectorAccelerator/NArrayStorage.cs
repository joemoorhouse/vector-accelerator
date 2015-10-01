using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator
{
    public abstract class NArrayStorage<T>
    {
        // Storage is column-major, i.e. an enumerator that passes through the contiguous memory block
        // goes down the first column first, then the second column and so on.
        // This is because cuBlas and MKL have (mostly) a column-major convention
        // The storage is not specified in this base-class, but we assume it will be a contiguous memory block.
        public readonly int RowCount; // rows of matrix 
        public readonly int ColumnCount; // columns of matrix
        public readonly int Length; // length of vector, or total number of elements in matrix
        
        public NArrayStorage() { }
        
        public NArrayStorage(int length)
        {
            RowCount = Length = length;
            ColumnCount = 1; // i.e. we create a column matrix
        }

        public NArrayStorage(int rowCount, int columnCount)
        {
            Length = rowCount * columnCount;
            RowCount = rowCount;
            ColumnCount = columnCount;
        }

        public abstract T this[int index] { get; set; }

        public abstract T First();

        public abstract bool Matches(NArrayStorage<T> other);

        public abstract NArrayStorage<T> Slice(int startIndex, int length);
    }
}
