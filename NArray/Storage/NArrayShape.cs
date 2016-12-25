using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NArray.Storage
{
    public class NArrayShape : INArrayShape
    {
        public int[] Dimensions { get; set; }

        public bool IsTranspose { get; set; }

        public int[] DimensionSizes { get; set; }

        public int[] TransposeDimensionSizes { get; set; }

        public bool IsScalar { get; set; }

        public bool IsArray { get; set; }

        public bool IsMatrix { get; set; }

        public int TotalSize { get; set; }

        public int Rows { get; set; }

        public int Columns { get; set; }

        public NArrayShape(int rows, int columns)
        {
            Rows = rows; Columns = columns;
            Dimensions = new int[] { rows, columns };
            DimensionSizes = new int[] { 1, rows };
            TransposeDimensionSizes = new int[] { rows, 1 };
            TotalSize = rows * columns;
            IsScalar = rows == 1 && columns == 1;
            IsArray = !IsScalar && (rows == 1 || columns == 1);
            IsMatrix = !IsScalar && !IsArray;
        }
    }
}
