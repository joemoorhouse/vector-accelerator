using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator.NArrayStorage;
using VectorAccelerator.DeferredExecution;
using VectorAccelerator.LinearAlgebraProviders;
using VectorAccelerator.Distributions;

namespace VectorAccelerator
{
    public interface IStorageCreator<T>
    {
        NArrayStorage<T> NewStorage(int rowCount, int columnCount);

        NArrayStorage<T> NewStorage(T[] array);

        NArrayStorage<T> NewStorage(T value);
    }
    
    public abstract class NArray<T>
    {
        public readonly int RowCount; // rows of matrix 
        public readonly int ColumnCount; // columns of matrix
        public readonly int Length; // length of vector, or total number of elements in matrix

        public bool IsScalar { get { return (Length == 1); } }
        public bool IsVector { get { return RowCount == 1 || ColumnCount == 1; } }
        
        protected NArrayStorage<T> _storage;

        public virtual NArrayStorage<T> Storage
        {
            get { return _storage; }
            set 
            {
                _storage = value;
                if (!StorageMatches(_storage)) throw new ArithmeticException("storage mismatch");
            }
        }

        public NArray(int length)
        {
            RowCount = Length = length;
            ColumnCount = 1;
            _storage = StorageCreator.NewStorage(length, 1);
        }

        public NArray(int rowCount, int columnCount)
        {
            RowCount = rowCount;
            ColumnCount = columnCount;
            Length = RowCount * ColumnCount;
            _storage = StorageCreator.NewStorage(rowCount, columnCount);
        }

        public NArray(T value)
        {
            RowCount = ColumnCount = Length = 1;
            _storage = StorageCreator.NewStorage(value);
        }

        public NArray(T[] value)
        {
            RowCount = Length = value.Length;
            ColumnCount = 1;
            _storage = StorageCreator.NewStorage(value);
        }

        public NArray(NArrayStorage<T> storage)
        {
            _storage = storage;
            RowCount = storage.RowCount;
            ColumnCount = storage.ColumnCount;
            Length = storage.Length;
        }

        protected abstract IStorageCreator<T> StorageCreator { get; }

        private bool StorageMatches(NArrayStorage<T> storage)
        {
            return storage.RowCount == RowCount && storage.ColumnCount == ColumnCount; 
        }
    }

    public enum StorageType { Host, Device, None }

    /// <summary>
    /// An N-dimensional array of double precision values.
    /// </summary>
    public class NArray : NArray<double>
    {
        public NArray(int length) : base(length) { }

        public NArray(int rowCount, int columnCount) : base(rowCount, columnCount) { }

        public NArray(double value) : base(value) { }

        public NArray(double[] array) : base(array) { }

        public NArray(NArrayStorage<double> storage) : base(storage) { }

        protected override IStorageCreator<double> StorageCreator
        {
            get 
            {
                if (this is LocalNArray)
                {
                    return new NullStorageCreator<double>();
                }
                else
                {
                    return new ManagedStorageCreator<double>();
                }
            }
        }

        /// <summary>
        /// To be used only for debugging
        /// </summary>
        public IEnumerable<double> DebugDataView
        {
            get 
            {
                if (Storage is ManagedStorage<double>)
                {
                    var managedStorage = Storage as ManagedStorage<double>;
                    return managedStorage.Array.Skip(managedStorage.ArrayStart).Take(managedStorage.Length);
                }
                else return null;
            }
        }

        public NArray Slice(int startIndex, int length)
        {
            return new NArray(Storage.Slice(startIndex, length));
        }

        public static NArray CreateLike(NArray a)
        {
            return new NArray(a.Storage.Length);
        }

        public static NArray CreateLike(NArray a, NArray b)
        {
            if (a.Storage.Length != b.Storage.Length)
                throw new ArgumentException("dimensions of a and b do not match");
            return new NArray(a.Storage.Length);
        }

        public static NArray CreateFromEnumerable(IEnumerable<double> enumerable)
        {
            var array = enumerable.ToArray();
            var newNArray = new NArray(array);
            // temporary
            return newNArray;
        }

        public static NArray CreateFromEnumerable(IEnumerable<int> enumerable)
        {
            return CreateFromEnumerable(enumerable.Select(i => (double)i));
        }

        public double First()
        {
            return Storage.First();
        }

        public void Assign(NArray operand)
        {
            ExecutionContext.Executor.Assign(this, operand);
        }

        public void Assign(Func<NArray> operand)
        {
            ExecutionContext.Executor.Assign(this, operand());
        }

        public static VectorAccelerator.DeferredExecution.DeferredExecutionContext DeferredExecution()
        {
            return DeferredExecution(new VectorExecutionOptions());
        }

        public static VectorAccelerator.DeferredExecution.DeferredExecutionContext DeferredExecution(VectorExecutionOptions options)
        {
            return new VectorAccelerator.DeferredExecution.DeferredExecutionContext(options);
        }

        public override string ToString()
        {
            if (IsScalar) return Storage.First().ToString();
            else return string.Format("NArray[{0}]", Length);
        }

        #region Binary Operators

        public static implicit operator NArray(double value)
        {
            return new NArray(value);
        }

        public static NArray operator +(NArray operand1, NArray operand2)
        {
            return ExecutionContext.Executor.ElementWiseAdd(operand1, operand2);
        }

        public static NArray operator -(NArray operand1, NArray operand2)
        {
            return ExecutionContext.Executor.ElementWiseSubtract(operand1, operand2);
        }

        public static NArray operator *(NArray operand1, NArray operand2)
        {
            return ExecutionContext.Executor.ElementWiseMultiply(operand1, operand2);
        }

        public static NArray operator /(NArray operand1, NArray operand2)
        {
            return ExecutionContext.Executor.ElementWiseDivide(operand1, operand2);
        }

        #endregion

        public static NArray operator -(NArray operand)
        {
            return ExecutionContext.Executor.ElementWiseNegate(operand);
        }

        public void Add(NArray operand)
        {
            ExecutionContext.Executor.Add(this, operand);
        }

        public static NArray CreateRandom(int length, ContinuousDistribution distribution)
        {
            var newNArray = new NArray(length);
            newNArray.FillRandom(distribution);
            return newNArray;
        }

        public void FillRandom(ContinuousDistribution distribution)
        {
            ExecutionContext.Executor.FillRandom(distribution, this);
        }

        /// <summary>
        /// If the element i of the boolean 'condition' vector, condition[i] is true, condition[i]
        /// is set to ifTrue[i], otherwise condition[i] is set to ifFalse[i]
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="ifTrue"></param>
        /// <param name="ifFalse"></param>
        public void AssignIfElse2(NArray condition, Func<NArray> ifTrue, Func<NArray> ifFalse)
        {

        }

        public void AssignIfElse(NArray condition, NArray ifTrue, NArray ifFalse)
        {

        }
    }

    /// <summary>
    /// An N-dimensional array of integers.
    /// </summary>
    public class NArrayInt : NArray<int>
    {
        public NArrayInt(int length)
            : base(length) { }

        protected override IStorageCreator<int> StorageCreator
        {
            get { throw new NotImplementedException(); }
        }
    }

}
