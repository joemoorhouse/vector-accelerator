using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VectorAccelerator.NArrayStorage;

namespace VectorAccelerator
{
    public class Assertions
    {
        public static void AssertSameShape<T,S>(NArray<T> a, NArray<S> b)
        {
            if (a.RowCount != b.RowCount || a.ColumnCount != b.ColumnCount) throw new ArgumentException("shape mismatch");
        }
        
        public static void AssertIsRow(NArray row, string paramName)
        {
            if (row.RowCount != 1) throw new ArgumentException("is not a row", paramName);
        }

        public static void AssertIsVectorOfLength(NArray vector, int length, string paramName)
        {
            if (!vector.IsVector || vector.Length != length) throw new ArgumentException(
                string.Format("is not a vector of length {0}", length), paramName);
        }

        public static void AssertIsColumn(NArray column, string paramName)
        {
            if (column.ColumnCount != 1) throw new ArgumentException("is not a column", paramName);
        }

        public static void AssertVectorsOfEqualLength(NArray a, NArray b)
        {
            if (!(a.IsVector && b.IsVector && a.Length == b.Length)) throw new ArgumentException("not equal length vectors");
        }

        public static void AssertRowMatchesMatrix(NArray matrix, NArray row, string matrixParam, string rowParam)
        {
            AssertIsRow(row, rowParam);
            if (row.ColumnCount != matrix.ColumnCount) throw new ArgumentException(string.Format("mismatching row {0} and matrix {1}", rowParam, matrixParam));
        }

        public static void AssertColumnMatchesMatrix(NArray matrix, NArray column, string matrixParam, string columnParam)
        {
            AssertIsColumn(column, columnParam);
            if (column.RowCount != matrix.RowCount)
                throw new ArgumentException(string.Format("mismatching column {0} and matrix {1}", columnParam, matrixParam));
        }
    }
}
