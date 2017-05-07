using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NArray.Storage;
using NArray.Interfaces;
using System.Collections;

namespace NArray
{
    public class NArrayBool : IEnumerable<bool>
    {
        INArrayBoolStorage _storage;

        public bool IsTranspose { get { return _storage.IsTranspose; } }

        public bool IsScalar { get { return _storage.IsScalar; } }

        public bool IsArray { get { return _storage.IsArray; } }

        public bool IsMatrix { get { return _storage.IsMatrix; } }

        public int TotalSize { get { return _storage.TotalSize; } }

        public int Rows { get { return _storage.Rows; } }

        public int Columns { get { return _storage.Columns; } }

        public INArrayBoolStorage Storage { get { return _storage; } set { _storage = value; } }

        public bool this[params int[] indices] { get { return _storage[indices]; } set { _storage[indices] = value; } }

        public NArrayBool(int rows, int columns, StorageType storageType = StorageType.Managed) 
        {
            if (storageType == StorageType.Managed)
            {
                Storage = new NArrayBoolStorage(rows, columns);
            }
        }

        public NArrayBool(bool scalarValue, StorageType storageType = StorageType.Managed)
        {
            if (storageType == StorageType.Managed)
            {
                Storage = new NArrayBoolStorage(1, 1);
                Storage[0] = scalarValue;
            }
        }

        public static implicit operator NArrayBool(bool value)
        {
            var nArray = new NArrayBool(1, 1, StorageType.Managed);
            nArray.Storage[0] = value;
            return nArray;
        }

        public IEnumerator<bool> GetEnumerator()
        {
            foreach (var value in _storage.Data) yield return value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
