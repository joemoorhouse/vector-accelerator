using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using VectorAccelerator;
using VectorAccelerator.NArrayStorage;
using VectorAccelerator.DeferredExecution;
using VectorAccelerator.LinearAlgebraProviders;
using VectorAccelerator.Distributions;

namespace VectorAccelerator
{    
    public enum Aggregator {  None, ElementwiseAdd };
    
    public static class StorageCreator
    {
        public static INArrayStorage<T> NewStorage<T>(StorageLocation location, int rowCount, int columnCount)
        {
            if (location == StorageLocation.Host) return new ManagedStorage<T>(rowCount, columnCount);
            else return null;
        }

        public static INArrayStorage<T> NewStorage<T>(StorageLocation location, T[] array)
        {
            if (location == StorageLocation.Host) return new ManagedStorage<T>(array);
            else return null;
        }

        public static INArrayStorage<T> NewStorage<T>(StorageLocation location, T[,] array)
        {
            if (location == StorageLocation.Host) return new ManagedStorage<T>(array);
            else return null;
        }

        public static INArrayStorage<T> NewStorage<T>(StorageLocation location, IEnumerable<T> enumerable)
        {
            if (location == StorageLocation.Host) return new ManagedStorage<T>(enumerable.ToArray());
            else return null;
        }

        public static INArrayStorage<T> NewStorage<T>(StorageLocation location, T value)
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

    public abstract class NArray<T> : INArray<T>
    {
        int _rows;
        int _columns;
        int _length;

        /// <summary>
        /// Number of rows
        /// </summary>
        public int Rows { get { return _rows; } }

        /// <summary>
        /// Number of columns
        /// </summary>
        public int Columns { get { return _columns; } }

        /// <summary>
        /// Length of vector, or total number of elements in matrix
        /// </summary>
        public int Length { get { return _length; } }

        public bool IsScalar { get { return (Length == 1); } }
        public bool IsVector { get { return (Length > 1) && (Rows == 1 || Columns == 1); } }
        public bool IsMatrix { get { return Rows > 1 && Columns > 1; } }
        
        protected INArrayStorage<T> _storage;

        public virtual INArrayStorage<T> Storage
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
            _rows = _length = length;
            _columns = 1;
            _storage = StorageCreator.NewStorage<T>(location, length, 1);
        }

        public NArray(StorageLocation location, int rowCount, int columnCount)
        {
            _rows = rowCount;
            _columns = columnCount;
            _length = _rows * _columns;
            _storage = StorageCreator.NewStorage<T>(location, rowCount, columnCount);
        }

        public NArray(StorageLocation location, T value)
        {
            _rows = _columns = _length = 1;
            _storage = StorageCreator.NewStorage(location, value);
        }

        public NArray(StorageLocation location, T[] value)
        {
            _rows = _length = value.Length;
            _columns = 1;
            _storage = StorageCreator.NewStorage(location, value);
        }

        public NArray(StorageLocation location, T[,] value)
        {
            _length = value.Length;
            _rows = value.GetLength(0);
            _columns = value.GetLength(1);
            _storage = StorageCreator.NewStorage(location, value);
        }

        public NArray(INArrayStorage<T> storage)
        {
            _storage = storage;
            _rows = storage.Rows;
            _columns = storage.Columns;
            _length = storage.Length;
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
                    return managedStorage.Data.Skip(managedStorage.DataStartIndex).Take(managedStorage.Length).ToArray();
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

        private bool StorageMatches(INArrayStorage<T> storage)
        {
            return storage.Rows == Rows && storage.Columns == Columns; 
        }

        public NArray<T> Slice(int startIndex, int length)
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
            get { return IsScalar ? this.First().ToString() : string.Format("NArray {0}x{1}", Rows, Columns); }
        }
        
        public NArray(StorageLocation location, int length) : base(location, length) { }

        public NArray(StorageLocation location, int rowCount, int columnCount) : base(location, rowCount, columnCount) { }

        public NArray(StorageLocation location, double value) : base(location, value) { }

        public NArray(StorageLocation location, double[] array) : base(location, array) { }

        public NArray(StorageLocation location, double[,] array) : base(location, array) { }

        public NArray(INArrayStorage<double> storage) : base(storage) { }

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

        public static IList<NArray> Evaluate(Func<NArray> function, IList<NArray> independentVariables,
            Aggregator aggregator, IList<NArray> existingStorage)
        {
            return VectorAccelerator.DeferredExecution.DeferredExecutionContext.Evaluate(function, independentVariables, null, aggregator, existingStorage);
        }

        public static NArray Evaluate(Func<NArray> function)
        {
            return VectorAccelerator.DeferredExecution.DeferredExecutionContext.Evaluate(function, new List<NArray>()).First();
        }

        public static void Evaluate(NArray result, Func<NArray> function)
        {
            VectorAccelerator.DeferredExecution.DeferredExecutionContext.Evaluate(function, new List<NArray>(), null, Aggregator.ElementwiseAdd, new List<NArray> { result }).First();
        }

        public static IList<NArray> Evaluate(VectorExecutionOptions vectorOptions, Func<NArray> function)
        {
            return VectorAccelerator.DeferredExecution.DeferredExecutionContext.Evaluate(function, new List<NArray>(), null, Aggregator.ElementwiseAdd, null, vectorOptions);
        }

        public static IList<NArray> Evaluate(Func<NArray> function, params NArray[] independentVariables)
        {
            return VectorAccelerator.DeferredExecution.DeferredExecutionContext.Evaluate(function, independentVariables);
        }

        public static IList<NArray> Evaluate(Func<NArray> function, StringBuilder expressionsOut, params NArray[] independentVariables)
        {
            return VectorAccelerator.DeferredExecution.DeferredExecutionContext.Evaluate(function, independentVariables, expressionsOut);
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
                (operand1.IsVector && operand2.IsVector && operand1.Rows != operand2.Rows))
            {
                var result = NArrayFactory.CreateLike(operand1, operand1.Rows, operand2.Columns);
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
            return ExecutionContext.Executor.RelativeOperation(operand1, operand2, RelativeOperation.LessThan);
        }

        public static NArrayBool operator <=(NArray operand1, NArray operand2)
        {
            return ExecutionContext.Executor.RelativeOperation(operand1, operand2, RelativeOperation.LessThanEquals);
        }

        //public static NArrayBool operator ==(NArray operand1, NArray operand2)
        //{
        //    return ExecutionContext.Executor.RelativeOperation(operand1, operand2, RelativeOperator.Equals);
        //}

        //public static NArrayBool operator !=(NArray operand1, NArray operand2)
        //{
        //    return ExecutionContext.Executor.RelativeOperation(operand1, operand2, RelativeOperator.NotEquals);
        //}

        public static NArrayBool operator >=(NArray operand1, NArray operand2)
        {
            return ExecutionContext.Executor.RelativeOperation(operand1, operand2, RelativeOperation.GreaterThanEquals);
        }

        public static NArrayBool operator >(NArray operand1, NArray operand2)
        {
            return ExecutionContext.Executor.RelativeOperation(operand1, operand2, RelativeOperation.GreaterThan);
        }

        #endregion

        public static NArray operator -(NArray operand)
        {
            return ExecutionContext.Executor.UnaryElementWiseOperation(operand, UnaryElementWiseOperation.Negate) as NArray;
        }

        public void Add(NArray operand)
        {
            ExecutionContext.Executor.ElementWiseAddInPlace(this, operand);
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

        public NArray GetColumn(int columnIndex)
        {
            var column = NArrayFactory.CreateLike(this, Rows, 1);
            this.Storage.CopySubMatrixTo(column.Storage, 0, 0, Rows, columnIndex, 0, 1);
            return column;
        }

        public IEnumerable<NArray> GetColumns()
        {
            for (int i = 0; i < Columns; ++i)
            {
                yield return GetColumn(i);
            }
        }

        public NArray ColumnAsReference(int columnIndex)
        {
            return new NArray(this.Storage.ColumnAsReference(columnIndex));
        }

        public void SetColumn(int columnIndex, NArray column)
        {
            Assertions.AssertColumnMatchesMatrix(this, column, "this", "column");
            column.Storage.CopySubMatrixTo(this.Storage, 0, 0, Rows, 0, columnIndex, 1);
        }

        public NArray GetRow(int rowIndex)
        {
            var row = NArrayFactory.CreateLike(this, 1, Columns);
            this.Storage.CopySubMatrixTo(row.Storage, rowIndex, 0, 1, 0, 0, Columns);
            return row;
        }

        public IEnumerable<NArray> GetRows(int rowIndex)
        {
            for (int i = 0; i < Rows; ++i)
            {
                yield return GetRow(i);
            }
        }

        public void SetRow(int rowIndex, NArray row)
        {
            Assertions.AssertRowMatchesMatrix(this, row, "this", "row");
            row.Storage.CopySubMatrixTo(this.Storage, 0, rowIndex, 1, 0, 0, Columns);
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
