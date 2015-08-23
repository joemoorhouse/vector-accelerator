using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator.NArrayStorage;

namespace VectorAccelerator
{
    public class NAray 
    {
        public readonly NArrayStorage<double> Storage;
        
        public readonly bool IsScalar;

        public NAray(int length)
        {
            Storage = new ManagedStorage<double>(length);
        }

        public NAray(double value)
        {
            IsScalar = true;
            Storage = new ManagedStorage<double>(value);
        }

        public NAray(double[] array)
        {
            Storage = new ManagedStorage<double>(array);
        }

        public static NAray CreateLike(NAray a)
        {
            return new NAray(a.Storage.Length);
        }

        public static NAray CreateLike(NAray a, NAray b)
        {
            if (a.Storage.Length != b.Storage.Length)
                throw new ArgumentException("dimensions of a and b do not match");
            return new NAray(a.Storage.Length);
        }

        public static NAray CreateRandom(int length, Random random)
        {
            var newNArray = new NAray(length);
            // temporary
            var array = (newNArray.Storage as ManagedStorage<double>).Array;
            for (int i = 0; i < length; ++i)
            {
                array[i] = random.NextDouble();
            }
            return newNArray;
        }

        public static NAray CreateFromEnumerable(IEnumerable<double> enumerable)
        {
            var array = enumerable.ToArray();
            var newNArray = new NAray(array);
            // temporary
            return newNArray;
        }

        public static NAray CreateFromEnumerable(IEnumerable<int> enumerable)
        {
            return CreateFromEnumerable(enumerable.Select(i => (double)i));
        }

        public double First()
        {
            return 0;
        }

        public int Length
        {
            get { return Storage.Length; } 
        }

        public void Assign(NAray operand)
        {
            ExecutionContext.Executor.Assign(this, operand);
        }

        public void Assign(Func<NAray> operand)
        {
            ExecutionContext.Executor.Assign(this, operand());
        }

        public static VectorAccelerator.DeferredExecution.DeferredExecutionContext DeferredExecution
        {
            get { return new VectorAccelerator.DeferredExecution.DeferredExecutionContext(); }
        }

        public override string ToString()
        {
            if (IsScalar) return Storage.First().ToString();
            else return string.Format("NArray[{0}]", Length);
        }

        #region Binary Operators

        public static NAray operator +(NAray operand1, NAray operand2)
        {
            return ExecutionContext.Executor.ElementWiseAdd(operand1, operand2);
        }

        public static NAray operator +(NAray operand1, double operand2)
        {
            return ExecutionContext.Executor.ElementWiseAdd(operand1, operand2);
        }

        public static NAray operator +(double operand1, NAray operand2)
        {
            return ExecutionContext.Executor.ElementWiseAdd(operand1, operand2);
        }

        public static NAray operator -(NAray operand1, NAray operand2)
        {
            return ExecutionContext.Executor.ElementWiseSubtract(operand1, operand2);
        }

        public static NAray operator -(NAray operand1, double operand2)
        {
            return ExecutionContext.Executor.ElementWiseSubtract(operand1, operand2);
        }

        public static NAray operator -(double operand1, NAray operand2)
        {
            return ExecutionContext.Executor.ElementWiseSubtract(operand1, operand2);
        }

        public static NAray operator *(NAray operand1, NAray operand2)
        {
            return ExecutionContext.Executor.ElementWiseMultiply(operand1, operand2);
        }

        public static NAray operator *(NAray operand1, double operand2)
        {
            return ExecutionContext.Executor.ElementWiseMultiply(operand1, operand2);
        }

        public static NAray operator *(double operand1, NAray operand2)
        {
            return ExecutionContext.Executor.ElementWiseMultiply(operand1, operand2);
        }

        public static NAray operator /(NAray operand1, NAray operand2)
        {
            return ExecutionContext.Executor.ElementWiseDivide(operand1, operand2);
        }

        public static NAray operator /(NAray operand1, double operand2)
        {
            return ExecutionContext.Executor.ElementWiseDivide(operand1, operand2);
        }

        public static NAray operator /(double operand1, NAray operand2)
        {
            return ExecutionContext.Executor.ElementWiseDivide(operand1, operand2);
        }

        #endregion

        public static NAray operator -(NAray operand)
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
        public void AssignIfElse2(NAray condition, Func<NAray> ifTrue, Func<NAray> ifFalse)
        {

        }

        public void AssignIfElse(NAray condition, NAray ifTrue, NAray ifFalse)
        {

        }
    }
}
