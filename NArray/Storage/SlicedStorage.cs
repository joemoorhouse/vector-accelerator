using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NArray.Storage
{
    class SlicedStorage : NArrayShape, INArrayStorage
    {
        double[] _data;
        int _dataStartIndex = 0;
        int _stride = 1;

        public double[] Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public int DataStart
        {
            get { return _dataStartIndex; }
            set { _dataStartIndex = value; }
        }

        public double this[int index]
        {
            get { return _data[index]; }
            set { _data[index] = value; }
        }

        public double this[params int[] indices] { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }

        public double this[int row, int column]
        {
            get { return _data[_dataStartIndex + _stride * column + row]; }
            set { _data[_dataStartIndex + _stride * column + row] = value; }
        }

        public int DataStartIndex
        {
            get { return _dataStartIndex; }
        }

        public SlicedStorage(int rows, int columns) : base(rows, columns) { }

        public SlicedStorage(double[] data, int dataStartIndex, int rows, int columns) : base(rows, columns)
        {
            _data = data;
            _dataStartIndex = dataStartIndex;
        }
    }
}
