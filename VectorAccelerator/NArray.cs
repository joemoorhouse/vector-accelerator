using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator.NArrayStorage;
using VectorAccelerator.DeferredExecution;
using VectorAccelerator.LinearAlgebraProviders;

namespace VectorAccelerator
{
    public class NArray 
    {
        private NArrayStorage<double> _storage;
        
        public virtual NArrayStorage<double> Storage
        {
            get { return _storage; }
            set { _storage = value; }
        }

        public bool IsScalar { get { return (Length == 1); } }

        public NArray(int length)
        {
            Storage = new ManagedStorage<double>(length);
        }

        public NArray()
        {
        }

        public NArray(NArrayStorage<double> storage)
        {
            Storage = storage;
        }

        public NArray(double value)
        {
            Storage = new ManagedStorage<double>(value);
        }

        public NArray(double[] array)
        {
            Storage = new ManagedStorage<double>(array);
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

        public static NArray CreateRandom(int length, Random random)
        {
            var newNArray = new NArray(length);
            // temporary
            var array = (newNArray.Storage as ManagedStorage<double>).Array;
            for (int i = 0; i < length; ++i)
            {
                array[i] = random.NextDouble();
            }
            return newNArray;
        }

        public static NArray CreateRandom(int length, IRandomNumberGenerator random)
        {
            var newNArray = new NArray(length);
            // temporary
            var array = (newNArray.Storage as ManagedStorage<double>).Array;
            random.NormalVector(array);
            return newNArray;
        }

        public void FillNormal(IRandomNumberGenerator random)
        {
            var array = (Storage as ManagedStorage<double>).Array;
            random.NormalVector(array);
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

        public virtual int Length
        {
            get { return Storage.Length; } 
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

    public class NArray<T> 
    {
        private NArrayStorage<T> _storage;
        
        public virtual NArrayStorage<T> Storage
        {
            get { return _storage; }
            set { _storage = value; }
        }

        public virtual int Length
        {
            get { return Storage.Length; }
        }

        public bool IsScalar { get { return (Length == 1); } }

        public NArray(int length)
        {
            Storage = new ManagedStorage<T>(length);
        }

        public NArray()
        {
        }

        public NArray(NArrayStorage<T> storage)
        {
            Storage = storage;
        }

        public NArray(T value)
        {
            Storage = new ManagedStorage<T>(value);
        }

        public NArray(T[] array)
        {
            Storage = new ManagedStorage<T>(array);
        }
        
        public NArray<T> this[NArray<int> indices]
        {
            get { return new NArray<T>(); }
        }

        public NArray<T> Slice(int startIndex, int length)
        {
            return new NArray<T>(Storage.Slice(startIndex, length));
        }

        public static NArray<T> CreateLike(NArray<T> a)
        {
            return new NArray<T>(a.Storage.Length);
        }

        public static NArray<T> CreateLike(NArray<T> a, NArray<T> b)
        {
            if (a.Storage.Length != b.Storage.Length)
                throw new ArgumentException("dimensions of a and b do not match");
            return new NArray<T>(a.Storage.Length);
        }

        public void FillNormal(IRandomNumberGenerator random)
        {
            var array = (Storage as ManagedStorage<T>).Array;
            //random.NormalVector(array);
        }

        public static NArray<T> CreateFromEnumerable(IEnumerable<T> enumerable)
        {
            var array = enumerable.ToArray();
            var newNArray = new NArray<T>(array);
            // temporary
            return newNArray;
        }


        public T First()
        {
            return Storage.First();
        }

        public void Assign(NArray<T> operand)
        {
            //ExecutionContext.Executor.Assign(this, operand);
        }

        public void Assign(Func<NArray<T>> operand)
        {
            //ExecutionContext.Executor.Assign(this, operand());
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

        public static implicit operator NArray<T>(T value)
        {
            return new NArray<T>(value);
        }

        public static NArray<T> operator +(NArray<T> operand1, NArray<T> operand2)
        {
            return new NArray<T>();
        }

        public static NArray<T> operator -(NArray<T> operand1, NArray<T> operand2)
        {
            return new NArray<T>();
        }

        public static NArray<T> operator *(NArray<T> operand1, NArray<T> operand2)
        {
            return new NArray<T>();
        }

        public static NArray<T> operator /(NArray<T> operand1, NArray<T> operand2)
        {
            return new NArray<T>();
        }

        #endregion

        public static NArray<T> operator -(NArray<T> operand)
        {
            return new NArray<T>();
        }
    }
}
