using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NArray.Storage
{
    public class NArrayStorage : NArrayShape, INArrayStorage
    {
        public double[] Data { get; set; }

        public int DataStart { get; set; }

        public double this[params int[] indices]
        {
            get
            {
                return Data[GetIndex(indices)];
            }
            set
            {
                Data[GetIndex(indices)] = value;
            }
        }

        private int GetIndex(int[] indices)
        {
            if (indices.Length == 1) return DataStart + indices[0];
            else if (indices.Length == 2) return DataStart + DimensionSizes[0] * indices[0] + indices[1];
            else throw new NotImplementedException();
        }

        public NArrayStorage(int rows, int columns) : base(rows, columns)
        {
            Data = new double[rows * columns];
        }
    }
}
