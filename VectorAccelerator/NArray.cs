using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using VectorAccelerator.NArrayStorage;
using VectorAccelerator.DeferredExecution;
using VectorAccelerator.LinearAlgebraProviders;
using VectorAccelerator.Distributions;

namespace VectorAccelerator
{    
    public static class StorageCreator
    {
        public static NArrayStorage<T> NewStorage<T>(StorageLocation location, int rowCount, int columnCount)
        {
            if (location == StorageLocation.Host) return new ManagedStorage<T>(rowCount, columnCount);
            else return null;
        }

        public static NArrayStorage<T> NewStorage<T>(StorageLocation location, T[] array)
        {
            if (location == StorageLocation.Host) return new ManagedStorage<T>(array);
            else return null;
        }

        public static NArrayStorage<T> NewStorage<T>(StorageLocation location, T[,] array)
        {
            if (location == StorageLocation.Host) return new ManagedStorage<T>(array);
            else return null;
        }

        public static NArrayStorage<T> NewStorage<T>(StorageLocation location, IEnumerable<T> enumerable)
        {
            if (location == StorageLocation.Host) return new ManagedStorage<T>(enumerable.ToArray());
            else return null;
        }

        public static NArrayStorage<T> NewStorage<T>(StorageLocation location, T value)
        {
            if (location == StorageLocation.Host) return new ManagedStorage<T>(value);
            else return null;
        }
    }

    /// <summary>
    /// Some syntactical sugar to allow transposed NArrays to be passed as arguments
    /// </summary>
    public class TransposedNArray
    {
        internal readonly NArray NArray;

        public TransposedNArray(NArray array)
        {
            NArray = array;
        }
    }

    public interface INArray
    {
    }

    public abstract class NArray<T> : INArray
    {
        public readonly int RowCount; // rows of matrix 
        public readonly int ColumnCount; // columns of matrix
        public readonly int Length; // length of vector, or total number of elements in matrix

        public bool IsScalar { get { return (Length == 1); } }
        public bool IsVector { get { return (Length > 1) && (RowCount == 1 || ColumnCount == 1); } }
        public bool IsMatrix { get { return RowCount > 1 && ColumnCount > 1; } }
        
        /// <summary>
        /// Whether this NArray is an independent variable for the purposes of differentiation
        /// </summary>
        public bool IsIndependentVariable { get { return _isIndependentVariable; } set { _isIndependentVariable = value; } }

        protected bool _isIndependentVariable = false;
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

        public NArray(StorageLocation location, int length)
        {
            RowCount = Length = length;
            ColumnCount = 1;
            _storage = StorageCreator.NewStorage<T>(location, length, 1);
        }

        public NArray(StorageLocation location, int rowCount, int columnCount)
        {
            RowCount = rowCount;
            ColumnCount = columnCount;
            Length = RowCount * ColumnCount;
            _storage = StorageCreator.NewStorage<T>(location, rowCount, columnCount);
        }

        public NArray(StorageLocation location, T value)
        {
            RowCount = ColumnCount = Length = 1;
            _storage = StorageCreator.NewStorage(location, value);
        }

        public NArray(StorageLocation location, T[] value)
        {
            RowCount = Length = value.Length;
            ColumnCount = 1;
            _storage = StorageCreator.NewStorage(location, value);
        }

        public NArray(StorageLocation location, T[,] value)
        {
            Length = value.Length;
            RowCount = value.GetLength(0);
            ColumnCount = value.GetLength(1);
            _storage = StorageCreator.NewStorage(location, value);
        }

        public NArray(NArrayStorage<T> storage)
        {
            _storage = storage;
            RowCount = storage.RowCount;
            ColumnCount = storage.ColumnCount;
            Length = storage.Length;
        }

        /// <summary>
        /// To be used only for debugging
        /// </summary>
        public IEnumerable<T> DebugDataView
        {
            get
            {
                if (Storage is ManagedStorage<T>)
                {
                    var managedStorage = Storage as ManagedStorage<T>;
                    return managedStorage.Array.Skip(managedStorage.ArrayStart).Take(managedStorage.Length).ToArray();
                }
                else return null;
            }
        }

        public T First()
        {
            return Storage.First();
        }

        public T GetValue(int index)
        {
            return ExecutionContext.Executor.GetValue(this, index);
        }

        private bool StorageMatches(NArrayStorage<T> storage)
        {
            return storage.RowCount == RowCount && storage.ColumnCount == ColumnCount; 
        }

        internal NArray<T> Slice(int startIndex, int length)
        {
            return NMath.CreateNArray(Storage.SliceAsReference(startIndex, length));
        }
    }

    public enum StorageLocation { Host, Device, None }

    /// <summary>
    /// An N-dimensional array of double precision values.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class NArray : NArray<double>
    {
        private string DebuggerDisplay
        {
            get { return IsScalar ? this.First().ToString() : string.Format("NArray {0}x{1}", RowCount, ColumnCount); }
        }
        
        public NArray(StorageLocation location, int length) : base(location, length) { }

        public NArray(StorageLocation location, int rowCount, int columnCount) : base(location, rowCount, columnCount) { }

        public NArray(StorageLocation location, double value) : base(location, value) { }

        public NArray(StorageLocation location, double[] array) : base(location, array) { }

        public NArray(StorageLocation location, double[,] array) : base(location, array) { }

        public NArray(NArrayStorage<double> storage) : base(storage) { }

        public static NArray CreateLike(NArray a)
        {
            return NArrayFactory.CreateLike(a);
        }

        public static NArray CreateScalar(double value)
        {
            return new NArray(StorageLocation.Host, new double[] { value });
        }

        public NArray Clone(MatrixRegion region = MatrixRegion.All)
        {
            return new NArray(Storage.Clone(region));
        }

        public void Assign(NArray operand)
        {
            ExecutionContext.Executor.Assign(this, operand);
        }

        public void Assign(Func<NArray> operand)
        {
            ExecutionContext.Executor.Assign(this, operand());
        }

        public void Assign(Func<NArrayBool> condition, Func<NArray> operand)
        {
            throw new NotImplementedException();
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
            //if (IsScalar) return Storage.First().ToString();
            return VectorAccelerator.InputOutput.MatrixStringHelper.ToMatrixString(this);
            //else return string.Format("NArray[{0}]", Length);
        }

        #region Binary Operators

        public static implicit operator NArray(double value)
        {
            return new NArray(StorageLocation.Host, value);
        }

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
                (operand1.IsVector && operand2.IsVector && operand1.RowCount != operand2.RowCount))
            {
                var result = NArrayFactory.CreateLike(operand1, operand1.RowCount, operand2.ColumnCount);
                ExecutionContext.Executor.MatrixMultiply(operand1, operand2, result);
                return result;
            }
            return ExecutionContext.Executor.ElementWiseMultiply(operand1, operand2) as NArray;
        }

        public static NArray operator /(NArray operand1, NArray operand2)
        {
            return ExecutionContext.Executor.ElementWiseDivide(operand1, operand2) as NArray; ;
        }

        #endregion

        #region Relational Operators

        public static NArrayBool operator <(NArray operand1, NArray operand2)
        {
            return ExecutionContext.Executor.RelativeOperation(operand1, operand2, RelativeOperator.LessThan);
        }

        public static NArrayBool operator <=(NArray operand1, NArray operand2)
        {
            return ExecutionContext.Executor.RelativeOperation(operand1, operand2, RelativeOperator.LessThanEquals);
        }

        public static NArrayBool operator ==(NArray operand1, NArray operand2)
        {
            return ExecutionContext.Executor.RelativeOperation(operand1, operand2, RelativeOperator.Equals);
        }

        public static NArrayBool operator !=(NArray operand1, NArray operand2)
        {
            return ExecutionContext.Executor.RelativeOperation(operand1, operand2, RelativeOperator.NotEquals);
        }

        public static NArrayBool operator >=(NArray operand1, NArray operand2)
        {
            return ExecutionContext.Executor.RelativeOperation(operand1, operand2, RelativeOperator.GreaterThanEquals);
        }

        public static NArrayBool operator >(NArray operand1, NArray operand2)
        {
            return ExecutionContext.Executor.RelativeOperation(operand1, operand2, RelativeOperator.GreaterThan);
        }

        #endregion

        public static NArray operator -(NArray operand)
        {
            return ExecutionContext.Executor.UnaryElementWiseOperation(operand, UnaryElementWiseOperation.Negate) as NArray;
        }

        public void Add(NArray operand)
        {
            ExecutionContext.Executor.Add(this, operand);
        }

        public static NArray CreateRandom(int length, ContinuousDistribution distribution)
        {
            var location = distribution.RandomNumberStream.Location;
            var newNArray = new NArray(location, length);
            newNArray.FillRandom(distribution);
            return newNArray;
        }

        public void FillRandom(ContinuousDistribution distribution)
        {
            ExecutionContext.Executor.FillRandom(distribution, this);
        }

        /// <summary>
        /// Syntactical sugar to mark that an NArray should be transposed when being passed as an argument.
        /// e.g. var c = a.T * a
        /// </summary>
        /// <returns></returns>
        public TransposedNArray T()
        {
            return new TransposedNArray(this);
        }

        public NArray this[NArrayInt indices]
        {
            get 
            {
                Assertions.AssertSameShape(this, indices);
                return ExecutionContext.Executor.Index(this, indices) as NArray; 
            }
        }

        public NArray this[NArrayBool condition]
        {
            set 
            {
                Assertions.AssertSameShape(this, condition);
                ExecutionContext.Executor.Assign(this, () => value, () => condition); 
            }
        }

        /// <summary>
        /// Returns shallow copy transpose
        /// </summary>
        /// <returns></returns>
        public NArray Transpose()
        {
            return new NArray(this.Storage.Transpose());     
        }

        public NArray Column(int columnIndex)
        {
            var column = NArrayFactory.CreateLike(this, RowCount, 1);
            this.Storage.CopySubMatrixTo(column.Storage, 0, 0, RowCount, columnIndex, 0, 1);
            return column;
        }

        public IEnumerable<NArray> Columns()
        {
            for (int i = 0; i < ColumnCount; ++i)
            {
                yield return Column(i);
            }
        }

        public NArray ColumnAsReference(int columnIndex)
        {
            return new NArray(this.Storage.ColumnAsReference(columnIndex));
        }

        public void SetColumn(int columnIndex, NArray column)
        {
            Assertions.AssertColumnMatchesMatrix(this, column, "this", "column");
            column.Storage.CopySubMatrixTo(this.Storage, 0, 0, RowCount, 0, columnIndex, 1);
        }

        public NArray Row(int rowIndex)
        {
            var row = NArrayFactory.CreateLike(this, 1, ColumnCount);
            this.Storage.CopySubMatrixTo(row.Storage, rowIndex, 0, 1, 0, 0, ColumnCount);
            return row;
        }

        public IEnumerable<NArray> Rows(int rowIndex)
        {
            for (int i = 0; i < RowCount; ++i)
            {
                yield return Row(i);
            }
        }

        public void SetRow(int rowIndex, NArray row)
        {
            Assertions.AssertRowMatchesMatrix(this, row, "this", "row");
            row.Storage.CopySubMatrixTo(this.Storage, 0, rowIndex, 1, 0, 0, ColumnCount);
        }

        /// <summary>
        /// Sums the elements of the array. 
        /// If running in deferred mode, will perform a reduction (minimising use of intermediate vectors).  
        /// </summary>
        /// <returns></returns>
        public double Sum()
        {
            return ExecutionContext.Executor.Sum(this);
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

}
