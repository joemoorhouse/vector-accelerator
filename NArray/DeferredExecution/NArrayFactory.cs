using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NArray.Interfaces;
using NArray.Storage;

namespace NArray.DeferredExecution
{
    public class NArrayFactory : INArrayFactory
    {
        public NArray NewNArray(int rows, int columns, double value)
        {
            var nArray = new NArray(rows, columns) { Storage = new NArrayStorage(rows, columns) };
            if (value != 0)
            {
                for (int i = 0; i < nArray.TotalSize; ++i) nArray.Storage.Data[i] = value;
            }
            return nArray;
        }

        public NArray NewNArrayLike(NArray other)
        {
            return new NArray(other.Rows, other.Columns) { Storage = new NArrayStorage(other.Rows, other.Columns) };
        }

        public NArray NewScalarNArray(double scalarValue)
        {
            var nArray = new NArray(1, 1) { Storage = new NArrayStorage(1, 1) };
            nArray.Storage.Data[0] = scalarValue;
            return nArray;
        }

        public LocalNArray NewLocalNArray(int index, int rows, int columns, double value)
        {
            var nArray = new LocalNArray(rows, columns);
            nArray.Index = index;
            return nArray;
        }

        public LocalNArray NewLocalNArrayLike(int index, NArray other)
        {
            return new LocalNArray(other.Rows, other.Columns)
            {
                Index = index
            };
        }

        public LocalNArray NewLocalScalarNArray(int index, double scalarValue)
        {
            var nArray = new LocalNArray(1, 1) { Storage = new NArrayStorage(1, 1) };
            nArray.Storage.Data[0] = scalarValue;
            nArray.Index = index;
            return nArray;
        }
    }
}
