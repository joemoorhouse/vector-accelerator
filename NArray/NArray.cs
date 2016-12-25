using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NArray.Interfaces;
using NArray.Storage;

namespace NArray
{
    public enum StorageType { Managed, None } 
    
    public partial class NArray
    {
        INArrayStorage _storage;

        public bool IsTranspose { get { return _storage.IsTranspose; } }

        public bool IsScalar { get { return _storage.IsScalar; } }

        public bool IsArray { get { return _storage.IsArray; } }

        public bool IsMatrix { get { return _storage.IsMatrix; } }

        public int TotalSize { get { return _storage.TotalSize; } }

        public int Rows { get { return _storage.Rows; } }

        public int Columns { get { return _storage.Columns; } }

        public INArrayStorage Storage { get { return _storage; } set { _storage = value; } }

        public double this[params int[] indices] {  get { return _storage[indices]; } set { _storage[indices] = value; } }

        public NArray(int rows, int columns, StorageType storageType = StorageType.Managed) 
        {
            if (storageType == StorageType.Managed)
            {
                Storage = new NArrayStorage(rows, columns);
            }
        }

        public NArray(double scalarValue, StorageType storageType = StorageType.Managed) 
        {
            if (storageType == StorageType.Managed)
            {
                Storage = new NArrayStorage(1, 1);
                Storage[0] = scalarValue;
            }
        }

        public NArray(StorageType storageType = StorageType.Managed) : this(0, storageType) { }

        public static implicit operator NArray(double value)
        {
            var nArray = new NArray(1, 1, StorageType.Managed);
            nArray.Storage[0] = value;
            return nArray;
        }

        #region Binary Operators

        public static NArray operator +(NArray operand1, NArray operand2)
        {
            return ExecutionContext.Executor.ElementWiseAdd(operand1, operand2) as NArray;
        }

        public static NArray operator -(NArray operand1, NArray operand2)
        {
            return ExecutionContext.Executor.ElementWiseSubtract(operand1, operand2) as NArray;
        }

        public static NArray operator *(NArray operand1, NArray operand2)
        {
            if ((operand1.IsMatrix && operand2.IsMatrix) ||
                (operand1.IsArray && operand2.IsArray && operand1.Rows != operand2.Rows))
            {
                //var result = NArrayFactory.CreateLike(operand1, operand1.Rows, operand2.Columns);
                //ExecutionContext.Executor.MatrixMultiply(operand1, operand2, result);
                //return result;
                throw new NotImplementedException();
            }
            return ExecutionContext.Executor.ElementWiseMultiply(operand1, operand2) as NArray;
        }

        public static NArray operator /(NArray operand1, NArray operand2)
        {
            return ExecutionContext.Executor.ElementWiseDivide(operand1, operand2) as NArray; ;
        }

        #endregion
    }
}
