using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VectorAccelerator.NArrayStorage;
using VectorAccelerator.Distributions;

namespace VectorAccelerator
{
    public class NArrayFactory
    {
        public readonly StorageLocation StorageLocation;

        public NArrayFactory(StorageLocation storageLocation)
        {
            StorageLocation = storageLocation;
        }

        #region Instance Methods

        public NArray CreateFromEnumerable(IEnumerable<double> enumerable)
        {
            return new NArray(StorageLocation, enumerable.ToArray());
        }

        public NArray CreateFromEnumerable(IEnumerable<int> enumerable)
        {
            return CreateFromEnumerable(enumerable.Select(i => (double)i));
        }

        public NArray<T> CreateNArray<T>(int length)
        {
            return CreateNArray<T>(StorageLocation, length);
        }

        public NArray CreateNArray(int rowCount, int columnCount)
        {
            return new NArray(StorageLocation, rowCount, columnCount);
        }

        public NArray CreateNArray(double[] array)
        {
            return CreateNArray(StorageLocation, array);
        }

        public NArray CreateNArray(double[,] array)
        {
            return CreateNArray(StorageLocation, array);
        }

        public NArrayInt CreateNArray(int[] array)
        {
            return CreateNArray(StorageLocation, array);
        }

        public RandomNumberStream CreateRandom(RandomNumberGeneratorType type = RandomNumberGeneratorType.MRG32K3A,
            int seed = 111)
        {
            return new RandomNumberStream(StorageLocation, type, seed);
        }

        #endregion

        #region Static Methods

        public static NArray<S> CreateLike<S, T>(NArray<T> a)
        {
            return CreateNArray<S>(GetStorageLocation<T>(a), a.Rows, a.Columns);
        }

        public static NArray<T> CreateLike<T>(NArray<T> a)
        {
            return CreateNArray<T>(GetStorageLocation<T>(a), a.Rows, a.Columns);
        }

        public static NArray<T> CreateLike<T>(NArray<T> a, int rowCount, int columnCount)
        {
            return CreateNArray<T>(GetStorageLocation<T>(a), rowCount, columnCount);
        }

        public static NArray CreateLike(NArray a, double[] array)
        {
            return CreateNArray(GetStorageLocation(a), array);
        }

        public static NArrayInt CreateLike(NArray a, int[] array)
        {
            return CreateNArray(GetStorageLocation(a), array);
        }

        public static NArray CreateLike(NArray a)
        {
            return CreateLike<double>(a) as NArray;
        }

        public static NArray CreateLike(NArray a, int length)
        {
            return CreateNArray<double>(GetStorageLocation(a), length) as NArray;
        }

        public static NArray CreateLike(NArray a, int rowCount, int columnCount)
        {
            return CreateNArray<double>(GetStorageLocation(a), rowCount, columnCount) as NArray;
        }

        public static NArray CreateConstantLike<T>(NArray<T> a, double constantValue)
        {
            var array = Enumerable.Repeat(constantValue, a.Length).ToArray();
            return CreateNArray(GetStorageLocation(a), array);
        }

        public static NArrayInt CreateConstantLike<T>(NArray<T> a, int constantValue)
        {
            var array = Enumerable.Repeat(constantValue, a.Length).ToArray();
            return CreateNArray(GetStorageLocation(a), array);
        }

        public static NArray<T> CreateNArray<T>(NArrayStorage<T> storage)
        {
            if (typeof(T) == typeof(double)) return new NArray(storage as INArrayStorage<double>) as NArray<T>;
            //else if (typeof(T) == typeof(int)) return new NArrayInt(storage as NArrayStorage<int>) as NArray<T>;
            else return null;
        }

        private static NArray<T> CreateNArray<T>(StorageLocation location, int length)
        {
            if (typeof(T) == typeof(double)) return new NArray(location, length) as NArray<T>;
            else if (typeof(T) == typeof(int)) return new NArrayInt(location, length) as NArray<T>;
            else if (typeof(T) == typeof(bool)) return new NArrayBool(location, length) as NArray<T>;
            else return null;
        }

        private static NArray<T> CreateNArray<T>(StorageLocation location, int rowCount, int columnCount)
        {
            if (typeof(T) == typeof(double)) return new NArray(location, rowCount, columnCount) as NArray<T>;
            else if (typeof(T) == typeof(int)) return new NArrayInt(location, rowCount * columnCount) as NArray<T>;
            else if (typeof(T) == typeof(bool)) return new NArrayBool(location, rowCount * columnCount) as NArray<T>;
            else return null;
        }

        private static NArray CreateNArray(StorageLocation location, double[] array)
        {
            return new NArray(location, array);
        }

        private static NArray CreateNArray(StorageLocation location, double[,] array)
        {
            return new NArray(location, array);
        }

        private static NArrayInt CreateNArray(StorageLocation location, int[] array)
        {
            return new NArrayInt(location, array);
        }

        public static StorageLocation GetStorageLocation<T>(NArray<T> operand)
        {
            var storageType = operand.Storage.GetType();
            if (storageType == typeof(ManagedStorage<T>))
            {
                return StorageLocation.Host;
            }
            else return StorageLocation.Device;
        }

        #endregion
    }
}
