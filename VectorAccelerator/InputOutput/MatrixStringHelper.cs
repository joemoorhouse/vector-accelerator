using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VectorAccelerator.NArrayStorage;

namespace VectorAccelerator.InputOutput
{
    /// <summary>
    /// Class for displaying NArray as text, taken from Math.NET Numerics (with thanks).
    /// </summary>
    public static class MatrixStringHelper
    {
        /// <summary>
        /// Returns a string that summarizes the content of this matrix.
        /// </summary>
        public static string ToMatrixString<T>(NArray<T> array, string format = null, IFormatProvider provider = null) where T : IFormattable
        {
            if (format == null)
            {
                format = "G6";
            }
            return ToMatrixString(array, 8, 4, 5, 2, 76, "..", "..", "..", "  ", Environment.NewLine, x => x.ToString(format, provider));
        }

        public static string ToMatrixString<T>(NArray<T> array, int upperRows, int lowerRows, int minLeftColumns, int rightColumns, int maxWidth,
            string horizontalEllipsis, string verticalEllipsis, string diagonalEllipsis,
            string columnSeparator, string rowSeparator, Func<T, string> formatValue)
        {
            return FormatStringArrayToString(
                ToMatrixStringArray(array, upperRows, lowerRows, minLeftColumns, rightColumns, maxWidth, columnSeparator.Length, horizontalEllipsis, verticalEllipsis, diagonalEllipsis, formatValue),
                columnSeparator, rowSeparator);
        }

        /// <summary>
        /// Returns a string 2D array that summarizes the content of this matrix.
        /// </summary>
        public static string[,] ToMatrixStringArray<T>(NArray<T> nArray, int upperRows, int lowerRows, int minLeftColumns, int rightColumns, int maxWidth, int padding,
            string horizontalEllipsis, string verticalEllipsis, string diagonalEllipsis, Func<T, string> formatValue)
        {
            upperRows = Math.Max(upperRows, 1);
            lowerRows = Math.Max(lowerRows, 0);
            minLeftColumns = Math.Max(minLeftColumns, 1);
            maxWidth = Math.Max(maxWidth, 12);

            int upper = nArray.RowCount <= upperRows ? nArray.RowCount : upperRows;
            int lower = nArray.RowCount <= upperRows ? 0 : nArray.RowCount <= upperRows + lowerRows ? nArray.RowCount - upperRows : lowerRows;
            bool rowEllipsis = nArray.RowCount > upper + lower;
            int rows = rowEllipsis ? upper + lower + 1 : upper + lower;

            int left = nArray.ColumnCount <= minLeftColumns ? nArray.ColumnCount : minLeftColumns;
            int right = nArray.ColumnCount <= minLeftColumns ? 0 : nArray.ColumnCount <= minLeftColumns + rightColumns ? nArray.ColumnCount - minLeftColumns : rightColumns;

            var columnsLeft = new List<Tuple<int, string[]>>();
            for (int j = 0; j < left; j++)
            {
                columnsLeft.Add(FormatColumn(nArray, j, rows, upper, lower, rowEllipsis, verticalEllipsis, formatValue));
            }

            var columnsRight = new List<Tuple<int, string[]>>();
            for (int j = 0; j < right; j++)
            {
                columnsRight.Add(FormatColumn(nArray, nArray.ColumnCount - right + j, rows, upper, lower, rowEllipsis, verticalEllipsis, formatValue));
            }

            int chars = columnsLeft.Sum(t => t.Item1 + padding) + columnsRight.Sum(t => t.Item1 + padding);
            for (int j = left; j < nArray.ColumnCount - right; j++)
            {
                var candidate = FormatColumn(nArray, j, rows, upper, lower, rowEllipsis, verticalEllipsis, formatValue);
                chars += candidate.Item1 + padding;
                if (chars > maxWidth)
                {
                    break;
                }
                columnsLeft.Add(candidate);
            }

            int cols = columnsLeft.Count + columnsRight.Count;
            bool colEllipsis = nArray.ColumnCount > cols;
            if (colEllipsis)
            {
                cols++;
            }

            var array = new string[rows, cols];
            int colIndex = 0;
            foreach (var column in columnsLeft)
            {
                for (int i = 0; i < column.Item2.Length; i++)
                {
                    array[i, colIndex] = column.Item2[i];
                }
                colIndex++;
            }
            if (colEllipsis)
            {
                int rowIndex = 0;
                for (var row = 0; row < upper; row++)
                {
                    array[rowIndex++, colIndex] = horizontalEllipsis;
                }
                if (rowEllipsis)
                {
                    array[rowIndex++, colIndex] = diagonalEllipsis;
                }
                for (var row = nArray.RowCount - lower; row < nArray.RowCount; row++)
                {
                    array[rowIndex++, colIndex] = horizontalEllipsis;
                }
                colIndex++;
            }
            foreach (var column in columnsRight)
            {
                for (int i = 0; i < column.Item2.Length; i++)
                {
                    array[i, colIndex] = column.Item2[i];
                }
                colIndex++;
            }
            return array;
        }

        private static Tuple<int, string[]> FormatColumn<T>(NArray<T> nArray, int column, int height, int upper, int lower, bool withEllipsis, string ellipsis, Func<T, string> formatValue)
        {
            var storage = nArray.Storage as ManagedStorage<T>;
            var c = new string[height];
            int index = 0;
            for (var row = 0; row < upper; row++)
            {
                c[index++] = formatValue(storage[row, column]);
            }
            if (withEllipsis)
            {
                c[index++] = "";
            }
            for (var row = nArray.RowCount - lower; row < nArray.RowCount; row++)
            {
                c[index++] = formatValue(storage[row, column]);
            }
            int w = c.Max(x => x.Length);
            if (withEllipsis)
            {
                c[upper] = ellipsis;
            }
            return new Tuple<int, string[]>(w, c);
        }

        private static string FormatStringArrayToString(string[,] array, string columnSeparator, string rowSeparator)
        {
            var rows = array.GetLength(0);
            var cols = array.GetLength(1);

            var widths = new int[cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    widths[j] = Math.Max(widths[j], array[i, j].Length);
                }
            }

            var sb = new StringBuilder();
            for (int i = 0; i < rows; i++)
            {
                sb.Append(array[i, 0].PadLeft(widths[0]));
                for (int j = 1; j < cols; j++)
                {
                    sb.Append(columnSeparator);
                    sb.Append(array[i, j].PadLeft(widths[j]));
                }
                sb.Append(rowSeparator);
            }
            return sb.ToString();
        }


    }
}
