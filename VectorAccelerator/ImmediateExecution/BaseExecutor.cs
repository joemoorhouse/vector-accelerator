using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator.LinearAlgebraProviders;
using VectorAccelerator.DeferredExecution.Expressions;

namespace VectorAccelerator
{
    public enum ExecutionMode { Instant, Deferred }

    public enum UnaryElementWiseOperation { ScaleInverse, Negate, Exp, Log, SquareRoot, Inverse, InverseSquareRoot, Normal, CumulativeNormal, InverseCumulativeNormal, ScaleOffset };

    public enum LogicalBinaryElementWiseOperation { And, Or };

    public abstract class BaseExecutor 
    {
        public ILinearAlgebraProvider Provider(StorageLocation storageLocation)
        {
            return _providers[(int)storageLocation];
        }
        
        public ILinearAlgebraProvider Provider<T>(params NArray<T>[] operands)
        {
            return GetProviderOrThrow(operands);
        }
        
        public BaseExecutor()
        {
            _providers = new ILinearAlgebraProvider[3];
            _providers[(int)StorageLocation.Host] = new IntelMKLLinearAlgebraProvider();
        }

        public abstract NArray<T> NewNArrayLike<T>(NArray<T> array);

        public abstract NArray<S> NewNArrayLike<S, T>(NArray<T> array);

        public virtual NArray<T> NewScalarNArray<T>(T scalarValue)
        {
            return ElementWise<T>().NewScalarNArray(scalarValue);
        }

        public abstract void DoScaleInverse<T>(NArray<T> a, T scale, NArray<T> result);

        public abstract void DoScaleOffset<T>(NArray<T> a, T scale, T offset, NArray<T> result);

        public abstract void DoBinaryElementWiseOperation<T>(NArray<T> a, NArray<T> b, NArray<T> result, ExpressionType operation);

        public abstract void DoUnaryElementWiseOperation<T>(NArray<T> a, NArray<T> result, UnaryElementWiseOperation operation);

        public void Convert<T>(int value, out T result)
        {
            ElementWise<T>().Convert(value, out result);
        }

        public void Negate<T>(T value, out T result)
        {
            ElementWise<T>().Negate(value, out result);
        }

        public T Add<T>(T a, T b)
        {
            return ElementWise<T>().Add(a, b);
        }

        public T Subtract<T>(T a, T b)
        {
            return ElementWise<T>().Subtract(a, b);
        }

        public T Multiply<T>(T a, T b)
        {
            return ElementWise<T>().Multiply(a, b);
        }

        public T Divide<T>(T a, T b)
        {
            return ElementWise<T>().Divide(a, b);
        }

        public ILinearAlgebraProvider<T> ElementWise<T>(params NArray<T>[] operands)
        {
            // check that we can operate on all NArrays (i.e. storage is all on host or all
            // on device) and then return the appropriate class to perform the (element-wise) 
            // arithmetic.
            if (operands.Length == 0) return _providers[(int)StorageLocation.Host] as ILinearAlgebraProvider<T>;
            return GetProviderOrThrow(operands) as ILinearAlgebraProvider<T>;
        }

        #region Binary Operations

        public abstract NArray<T> ElementWiseAdd<T>(NArray<T> operand1, NArray<T> operand2);

        public abstract NArray<T> ElementWiseSubtract<T>(NArray<T> operand1, NArray<T> operand2);

        public abstract NArray<T> ElementWiseMultiply<T>(NArray<T> operand1, NArray<T> operand2);

        public abstract NArray<T> ElementWiseDivide<T>(NArray<T> operand1, NArray<T> operand2);

        public virtual void ElementWiseAddInPlace<T>(NArray<T> operand1, NArray<T> operand2)
        {
            if (operand2.IsScalar)
            {
                T scale; Convert(1, out scale);
                DoScaleOffset(operand1, scale, operand2.First(), operand1);
            }
            else
            {
                DoBinaryElementWiseOperation(operand1, operand2, operand1, ExpressionType.Add);
            }
        }

       
        public NArrayBool RelativeOperation<T>(NArray<T> operand1, NArray<T> operand2, RelativeOperation op)
        {
            if (operand1.IsScalar) throw new ArgumentException();
            var result = NewNArrayLike<bool, T>(operand1) as NArrayBool;
            if (operand2.IsScalar)
            {
                ElementWise<T>(operand1).RelativeOperation(operand1, operand2.First(), result, op);
            }
            else
            {
                ElementWise<T>(operand1, operand2).RelativeOperation(operand1, operand2, result, op);
            }
            return result;
        }

        #endregion

        //#region Unary Operations

        public virtual NArray<T> UnaryElementWiseOperation<T>(NArray<T> operand, UnaryElementWiseOperation operation)
        {
            var result = NewNArrayLike(operand);
            DoUnaryElementWiseOperation<T>(operand, result, operation);
            return result;
        }

        //public NArray<T> ElementWiseNegate<T>(NArray<T> operand)
        //{
        //    var result = NewNArrayLike(operand);
        //    T scale; Convert(-1, out scale);
        //    T offset; Convert(0, out offset);
        //    DoScaleOffset(operand, scale, offset, result);
        //    return result;
        //}

        private ILinearAlgebraProvider GetProviderOrThrow<T>(params NArray<T>[] operands)
        {
            var storageLocation = GetStorageLocationOrThrow(operands);
            return _providers[(int)storageLocation];
        }

        private static StorageLocation GetStorageLocationOrThrow<T>(params NArray<T>[] operands)
        {
            var firstType = GetStorageLocation(operands.First());
            if (operands.Any(o => GetStorageLocation(o) != firstType))
                throw new ArgumentException("Cannot inter-operate NArrays with storage on Host with NArrays with storage on Device.");

            return firstType;
        }

        private static StorageLocation GetStorageLocation<T>(NArray<T> operand)
        {
            return NArrayFactory.GetStorageLocation<T>(operand);
        }

        private ILinearAlgebraProvider[] _providers;
    }
}
