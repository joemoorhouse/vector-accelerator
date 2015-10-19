using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator.LinearAlgebraProviders;
using VectorAccelerator.Distributions;
using VectorAccelerator.NArrayStorage;

namespace VectorAccelerator
{
    public enum ExecutionMode { Instant, Deferred }

    public enum BinaryElementWiseOperation { Add, Subtract, Multiply, Divide };
    
    public enum UnaryElementWiseOperation { Inverse, Negate, Exp, Log, SquareRoot, InverseSquareRoot, CumulativeNormal, InverseCumulativeNormal };

    public enum LogicalBinaryElementWiseOperation { And, Or };

    public abstract class BaseExecutor 
    {
        public abstract ILinearAlgebraProvider Provider { get; }
        
        public abstract NArray<T> NewNArrayLike<T>(NArray<T> array);

        public abstract NArray<S> NewNArrayLike<S, T>(NArray<T> array);

        public NArray<T> NewScalarNArray<T>(T scalarValue)
        {
            return ElementWise<T>().NewScalarNArray(scalarValue);
        }

        public abstract void DoScaleOffset<T>(NArray<T> a, T scale, T offset, NArray<T> result);

        public abstract void DoBinaryElementWiseOperation<T>(NArray<T> a, NArray<T> b, NArray<T> result, BinaryElementWiseOperation operation);

        public abstract void DoUnaryElementWiseOperation<T>(NArray<T> a, NArray<T> result, UnaryElementWiseOperation operation);

        //public abstract void DoRelativeElementWiseOperation<T>(NArray<T> a, NArray<T> result, RelativeOperator operation);

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

        public IElementWise<T> ElementWise<T>()
        {
            return Provider as IElementWise<T>;
        }

        //public void Assign(NArray operand1, NArray operand2)
        //{
        //    var managedStorage = operand2.Storage as ManagedStorage<double>;
        //    operand1.Storage = new ManagedStorage<double>((double[])managedStorage.Array.Clone(),
        //        managedStorage.ArrayStart, managedStorage.Length);
        //}

        //#region Creation

        //public NArrayInt ConstantLike<T>(int constantValue, NArray<T> array)
        //{
        //    return new NArrayInt(array.Length, constantValue);
        //}

        //#endregion

        #region Binary Operations

        public NArray<T> ElementWiseAdd<T>(NArray<T> operand1, NArray<T> operand2)
        {
            NArray<T> result = null;
            if (operand1.IsScalar && operand2.IsScalar)
            {
                result = NewScalarNArray(Add(operand1.First(), operand2.First()));
            }
            else if (!operand1.IsScalar && !operand2.IsScalar)
            {
                result = NewNArrayLike(operand1);
                DoBinaryElementWiseOperation(operand1, operand2, result, BinaryElementWiseOperation.Add);
            }
            else
            {
                T scale; Convert(1, out scale);
                if (operand1.IsScalar)
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
            if (operand1.IsScalar && operand2.IsScalar)
            {
                result = NewScalarNArray(Subtract(operand1.First(), operand2.First()));
            }
            else if (!operand1.IsScalar && !operand2.IsScalar)
            {
                result = NewNArrayLike(operand1);
                DoBinaryElementWiseOperation(operand1, operand2, result, BinaryElementWiseOperation.Subtract);
            }
            else
            {
                if (operand1.IsScalar)
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
            if (operand1.IsScalar && operand2.IsScalar)
            {
                result = NewScalarNArray(Multiply(operand1.First(), operand2.First()));
            }
            else if (!operand1.IsScalar && !operand2.IsScalar)
            {
                result = NewNArrayLike(operand1);
                DoBinaryElementWiseOperation(operand1, operand2, result, BinaryElementWiseOperation.Multiply);
            }
            else
            {
                T offset; Convert(0, out offset);
                if (operand1.IsScalar)
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
            if (operand1.IsScalar && operand2.IsScalar)
            {
                result = NewScalarNArray(Divide(operand1.First(), operand2.First()));
            }
            else if (!operand1.IsScalar && !operand2.IsScalar)
            {
                result = NewNArrayLike(operand1);
                DoBinaryElementWiseOperation(operand1, operand2, result, BinaryElementWiseOperation.Divide);
            }
            else
            {
                T offset; Convert(0, out offset);
                if (operand1.IsScalar)
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
                ElementWise<T>().RelativeOperation(operand1, operand2.First(), result, op);
            }
            else
            {
                ElementWise<T>().RelativeOperation(operand1, operand2, result, op);
            }
            return result;
        }

        //public void MatrixMultiply(NArray operand1, NArray operand2, NArray result)
        //{
        //    _provider.MatrixMultiply(operand1, operand2, result);
        //}

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

        //public void Add(NArray operand1, NArray operand2)
        //{
        //    _provider.ElementWiseOperation(operand1, operand2, operand1, ElementWiseOperation.Add);
        //}

        //public IDisposable CreateRandomNumberStream(RandomNumberGeneratorType type, int seed)
        //{
        //    return _provider.CreateRandomNumberStream(type, seed);
        //}

        //public void FillRandom(ContinuousDistribution distribution, NArray operand)
        //{
        //    _provider.FillRandom(distribution, operand);
        //}

        //public NArray Index(NArrayInt indices)
        //{
        //    var result = new NArray(indices.Storage.Length);
        //    return result;
        //}

        //#endregion
    }
}
