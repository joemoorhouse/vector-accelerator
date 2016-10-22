using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator
{
    public class NArrayInt : NArray<int>
    {
        public NArrayInt(StorageLocation location, int length)
            : base(location, length) { }

        public NArrayInt(StorageLocation location, int[] array) : base(location, array) { }

        public void Assign(Func<NArrayBool> condition, Func<NArrayInt> operand)
        {   
            //ExecutionContext.Executor.Index<int>(condition(), operand());
            //throw new NotImplementedException();
        }

        public NArrayInt this[NArrayBool condition]
        {
            set { ExecutionContext.Executor.Assign(this, () => value, () => condition); }
        }

        #region Binary Operators

        public static implicit operator NArrayInt(int value)
        {
            return new NArrayInt(StorageLocation.Host, value);
        }

        public static NArrayInt operator +(NArrayInt operand1, NArrayInt operand2)
        {
            return ExecutionContext.Executor.ElementWiseAdd(operand1, operand2) as NArrayInt;
        }

        public static NArrayInt operator -(NArrayInt operand1, NArrayInt operand2)
        {
            return ExecutionContext.Executor.ElementWiseSubtract(operand1, operand2) as NArrayInt;
        }

        public static NArrayInt operator *(NArrayInt operand1, NArrayInt operand2)
        {
            return ExecutionContext.Executor.ElementWiseMultiply(operand1, operand2) as NArrayInt;
        }

        public static NArrayInt operator /(NArrayInt operand1, NArrayInt operand2)
        {
            return ExecutionContext.Executor.ElementWiseDivide(operand1, operand2) as NArrayInt;
        }

        #endregion

        public static NArrayInt operator >>(NArrayInt operand, int shift)
        {
            return ExecutionContext.Executor.RightShift(operand, shift) as NArrayInt; ;
        }

        public static NArrayInt operator <<(NArrayInt operand, int shift)
        {
            return ExecutionContext.Executor.LeftShift(operand, shift) as NArrayInt; ;
        }

        #region Relational Operators

        public static NArrayBool operator <(NArrayInt operand1, NArrayInt operand2)
        {
            return ExecutionContext.Executor.RelativeOperation(operand1, operand2, RelativeOperator.LessThan);
        }

        public static NArrayBool operator <=(NArrayInt operand1, NArrayInt operand2)
        {
            return ExecutionContext.Executor.RelativeOperation(operand1, operand2, RelativeOperator.LessThanEquals);
        }

        //public static NArrayBool operator ==(NArrayInt operand1, NArrayInt operand2)
        //{
        //    return ExecutionContext.Executor.RelativeOperation(operand1, operand2, RelativeOperator.Equals);
        //}

        //public static NArrayBool operator !=(NArrayInt operand1, NArrayInt operand2)
        //{
        //    return ExecutionContext.Executor.RelativeOperation(operand1, operand2, RelativeOperator.NotEquals);
        //}

        public static NArrayBool operator >=(NArrayInt operand1, NArrayInt operand2)
        {
            return ExecutionContext.Executor.RelativeOperation(operand1, operand2, RelativeOperator.GreaterThanEquals);
        }

        public static NArrayBool operator >(NArrayInt operand1, NArrayInt operand2)
        {
            return ExecutionContext.Executor.RelativeOperation(operand1, operand2, RelativeOperator.GreaterThan);
        }

        #endregion
    }
}
