using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator.LinearAlgebraProviders;
using VectorAccelerator.Distributions;
using VectorAccelerator.NArrayStorage;
using System.Linq.Expressions;

namespace VectorAccelerator
{
    public enum ExecutionMode { Instant, Deferred }

    //public enum BinaryElementWiseOperation { Add, Subtract, Multiply, Divide };
    
    public enum UnaryElementWiseOperation { Inverse, Negate, Exp, Log, SquareRoot, InverseSquareRoot, CumulativeNormal, InverseCumulativeNormal, ScaleOffset };

    public enum LogicalBinaryElementWiseOperation { And, Or };

    public abstract class BaseExecutor 
    {
        public LinearAlgebraProvider Provider(StorageLocation storageLocation)
        {
            return _providers[(int)storageLocation];
        }
        
        public LinearAlgebraProvider Provider<T>(params NArray<T>[] operands)
        {
            return GetProviderOrThrow(operands);
        }
        
        public BaseExecutor()
        {
            _providers = new LinearAlgebraProvider[3];
            _providers[(int)StorageLocation.Host] = new IntelMKLLinearAlgebraProvider();
        }

        public abstract NArray<T> NewNArrayLike<T>(NArray<T> array);

        public abstract NArray<S> NewNArrayLike<S, T>(NArray<T> array);

        public NArray<T> NewScalarNArray<T>(T scalarValue)
        {
            return ElementWise<T>().NewScalarNArray(scalarValue);
        }

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

        public IElementWise<T> ElementWise<T>(params NArray<T>[] operands)
        {
            // check that we can operate on all NArrays (i.e. storage is all on host or all
            // on device) and then return the appropriate class to perform the (element-wise) 
            // arithmetic.
            if (operands.Length == 0) return _providers[(int)StorageLocation.Host] as IElementWise<T>;
            return GetProviderOrThrow(operands) as IElementWise<T>;
        }

        #region Binary Operations

        public NArray<T> ElementWiseAdd<T>(NArray<T> operand1, NArray<T> operand2)
        {
            NArray<T> result = null;
            if (operand1.IsScalar && operand2.IsScalar)
            {
                result = NewScalarNArray(Add(operand1.First(), operand2.First()));
            }
            else if (!IsScalarConstant(operand1) && !IsScalarConstant(operand2))
            {
                result = NewNArrayLike(operand1);
                DoBinaryElementWiseOperation(operand1, operand2, result, ExpressionType.Add);
            }
            else
            {
                T scale; Convert(1, out scale);
                if (IsScalarConstant(operand1))
                {
                    result = NewNArrayLike(operand2);
                    DoScaleOffset(operand2, scale, operand1.First(), result);
                }
                else
                {
                    result = NewNArrayLike(operand1);
                    DoScaleOffset(operand1, scale, operand2.First(), result);
                }
            }
            return result;
        }

        public NArray<T> ElementWiseSubtract<T>(NArray<T> operand1, NArray<T> operand2)
        {
            NArray<T> result = null;
            if (IsScalarConstant(operand1) && IsScalarConstant(operand2))
            {
                result = NewScalarNArray(Subtract(operand1.First(), operand2.First()));
            }
            else if (!IsScalarConstant(operand1) && !IsScalarConstant(operand2))
            {
                result = NewNArrayLike(operand1);
                DoBinaryElementWiseOperation(operand1, operand2, result, ExpressionType.Subtract);
            }
            else
            {
                if (IsScalarConstant(operand1))
                {
                    result = NewNArrayLike(operand2);
                    T scale; Convert(-1, out scale);
                    DoScaleOffset(operand2, scale, operand1.First(), result);
                }
                else
                {
                    result = NewNArrayLike(operand1);
                    T scale; Convert(1, out scale);
                    T offset; Negate(operand2.First(), out offset);
                    DoScaleOffset(operand1, scale, offset, result);
                }
            }
            return result;
        }

        public NArray<T> ElementWiseMultiply<T>(NArray<T> operand1, NArray<T> operand2)
        {
            NArray<T> result = null;
            if (IsScalarConstant(operand1) && IsScalarConstant(operand2))
            {
                result = NewScalarNArray(Multiply(operand1.First(), operand2.First()));
            }
            else if (!IsScalarConstant(operand1) && !IsScalarConstant(operand2))
            {
                result = NewNArrayLike(operand1.Length > operand2.Length ? operand1 : operand2);
                DoBinaryElementWiseOperation(operand1, operand2, result, ExpressionType.Multiply);
            }
            else
            {
                T offset; Convert(0, out offset);
                if (IsScalarConstant(operand1))
                {
                    result = NewNArrayLike(operand2);
                    DoScaleOffset(operand2, operand1.First(), offset, result);
                }
                else
                {
                    result = NewNArrayLike(operand1);
                    DoScaleOffset(operand1, operand2.First(), offset, result);
                }
            }
            return result;
        }

        public NArray<T> ElementWiseDivide<T>(NArray<T> operand1, NArray<T> operand2)
        {
            NArray<T> result = null;
            if (IsScalarConstant(operand1) && IsScalarConstant(operand2))
            {
                result = NewScalarNArray(Divide(operand1.First(), operand2.First()));
            }
            else if (!IsScalarConstant(operand1) && !IsScalarConstant(operand2))
            {
                result = NewNArrayLike(operand1);
                DoBinaryElementWiseOperation(operand1, operand2, result, ExpressionType.Divide);
            }
            else
            {
                T offset; Convert(0, out offset);
                if (IsScalarConstant(operand1))
                {
                    result = NewNArrayLike(operand2);
                    if (typeof(T) == typeof(double))
                    {
                        DoUnaryElementWiseOperation(operand2, result, VectorAccelerator.UnaryElementWiseOperation.Inverse);
                        DoScaleOffset(result, operand1.First(), offset, result);
                    }
                    else throw new NotImplementedException();
                }
                else
                {
                    result = NewNArrayLike(operand1);
                    if (typeof(T) == typeof(double))
                    {
                        DoScaleOffset(operand1 as NArray, 1.0 / (operand2 as NArray).First(), 0, (result as NArray));
                    }
                    else if (typeof(T) == typeof(int))
                    {
                        throw new NotImplementedException();
                    }
                }
            }
            return result;
        }

        public NArrayBool RelativeOperation<T>(NArray<T> operand1, NArray<T> operand2, RelativeOperator op)
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

        public NArray<T> UnaryElementWiseOperation<T>(NArray<T> operand, UnaryElementWiseOperation operation)
        {
            var result = NewNArrayLike(operand);
            DoUnaryElementWiseOperation<T>(operand, result, operation);
            return result;
        }

        public NArray<T> ElementWiseNegate<T>(NArray<T> operand)
        {
            var result = NewNArrayLike(operand);
            T scale; Convert(-1, out scale);
            T offset; Convert(0, out offset);
            DoScaleOffset(operand, scale, offset, result);
            return result;
        }

        private LinearAlgebraProvider GetProviderOrThrow<T>(params NArray<T>[] operands)
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

        /// <summary>
        /// Is the array a scalar that does not act as an independent variable in any differentiation.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        private bool IsScalarConstant<T>(NArray<T> array)
        {
            return array.IsScalar && !array.IsIndependentVariable;
        }

        private LinearAlgebraProvider[] _providers;
    }
}
