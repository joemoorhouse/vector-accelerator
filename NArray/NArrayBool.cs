using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NArray.Storage;

namespace NArray
{
    public class NArrayBool : NArray
    {
        public NArrayBool(int rows, int columns, StorageType storageType = StorageType.Managed) 
        {
            if (storageType == StorageType.Managed)
            {
                Storage = new NArrayStorage(rows, columns);
            }
        }

        public NArrayBool(double scalarValue, StorageType storageType = StorageType.Managed)
        {
            if (storageType == StorageType.Managed)
            {
                Storage = new NArrayStorage(1, 1);
                Storage[0] = scalarValue;
            }
        }


        public static implicit operator NArrayBool(bool value)
        {
            var nArray = new NArrayBool(1, 1, StorageType.Managed);
            nArray.Storage[0] = Convert.ToDouble(value);
            return nArray;
        }
    }
}
