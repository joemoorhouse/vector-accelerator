using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator.LinearAlgebraProviders;
using VectorAccelerator.Distributions;
using VectorAccelerator.NArrayStorage;
using VectorAccelerator.DeferredExecution.Expressions;

namespace VectorAccelerator
{
    public class ImmediateExecutor : BaseExecutor, IExecutor
    {
        public void Assign<T>(NArray<T> operand1, NArray<T> operand2)
        {
            var managedStorage = operand2.Storage as ManagedStorage<T>;
            operand1.Storage = new ManagedStorage<T>((T[])managedStorage.Array.Clone(), 
                managedStorage.ArrayStart, managedStorage.Length);
        }

        public void Assign<T>(NArray<T> operand1, Func<NArray<T>> operand2, Func<NArrayBool> condition)
        {
            // simple assignment: we evaluate the functions to generate full vectors and assign a portion.
            var op2 = operand2();
            Provider(operand1, op2).Assign(op2, condition(), operand1);
        }

        public override NArray<T> NewNArrayLike<T>(NArray<T> array)
        {
            return NArrayFactory.CreateLike(array);
        }

        public override NArray<S> NewNArrayLike<S, T>(NArray<T> array)
        {
            return NArrayFactory.CreateLike<S, T>(array);
        }

        public override void DoScaleInverse<T>(NArray<T> a, T scale, NArray<T> result)
        {
            ElementWise<T>(a).ScaleInverse(a, scale, result);
        }

        public override void DoScaleOffset<T>(NArray<T> a, T scale, T offset, NArray<T> result)
        {
            ElementWise<T>(a).ScaleOffset(a, scale, offset, result);
        }

        public override void DoBinaryElementWiseOperation<T>(NArray<T> a, NArray<T> b, NArray<T> result, ExpressionType operation)
        {
            ElementWise<T>(a, b, result).BinaryElementWiseOperation(a, b, result, operation);
        }

        public NArrayBool LogicalOperation(NArrayBool operand1, NArrayBool operand2, LogicalBinaryElementWiseOperation op)
        {
            var result = NewNArrayLike(operand1) as NArrayBool;
            Provider(operand1, operand2).LogicalOperation(operand1, operand2, result, op);
            return result;
        }

        public override void DoUnaryElementWiseOperation<T>(NArray<T> a, NArray<T> result, UnaryElementWiseOperation operation)
        {
            ElementWise<T>(a).UnaryElementWiseOperation(a, result, operation);
        }

        #region Creation

        public NArrayInt ConstantLike<T>(int constantValue, NArray<T> array)
        {
            return NArrayFactory.CreateConstantLike(array, constantValue);
        }

        #endregion

        public T GetValue<T>(NArray<T> array, int index)
        {
            return array.Storage[index];
        }

        #region Binary Operations

        public override NArray<T> ElementWiseAdd<T>(NArray<T> operand1, NArray<T> operand2) // immediate-mode version
        {
            NArray<T> result = null;
            if (operand1.IsScalar && operand2.IsScalar) result = NewScalarNArray(Add(operand1.First(), operand2.First()));
            else if (!operand1.IsScalar && !operand2.IsScalar)
            {
                result = NewNArrayLike(operand1);
                DoBinaryElementWiseOperation(operand1, operand2, result, ExpressionType.Add);
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

        public override NArray<T> ElementWiseSubtract<T>(NArray<T> operand1, NArray<T> operand2)
        {
            NArray<T> result = null;
            if (operand1.IsScalar && operand2.IsScalar)
            {
                result = NewScalarNArray(Subtract(operand1.First(), operand2.First()));
            }
            else if (!operand1.IsScalar && !operand2.IsScalar)
            {
                result = NewNArrayLike(operand1);
                DoBinaryElementWiseOperation(operand1, operand2, result, ExpressionType.Subtract);
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

        public override NArray<T> ElementWiseMultiply<T>(NArray<T> operand1, NArray<T> operand2)
        {
            NArray<T> result = null;
            if (operand1.IsScalar && operand2.IsScalar)
            {
                result = NewScalarNArray(Multiply(operand1.First(), operand2.First()));
            }
            else if (!operand1.IsScalar && !operand2.IsScalar)
            {
                result = NewNArrayLike(operand1.Length > operand2.Length ? operand1 : operand2);
                DoBinaryElementWiseOperation(operand1, operand2, result, ExpressionType.Multiply);
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

        public override NArray<T> ElementWiseDivide<T>(NArray<T> operand1, NArray<T> operand2)
        {
            NArray<T> result = null;
            if (operand1.IsScalar && operand2.IsScalar)
            {
                result = NewScalarNArray(Divide(operand1.First(), operand2.First()));
            }
            else if (!operand1.IsScalar && !operand2.IsScalar)
            {
                result = NewNArrayLike(operand1);
                DoBinaryElementWiseOperation(operand1, operand2, result, ExpressionType.Divide);
            }
            else
            {
                T offset; Convert(0, out offset);
                if (operand1.IsScalar)
                {
                    result = NewNArrayLike(operand2);
                    if (typeof(T) == typeof(double))
                    {
                        DoScaleInverse(operand2, operand1.First(), result);
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

        public NArray<int> LeftShift(NArray<int> operand, int shift)
        {
            var result = NewNArrayLike(operand);
            Provider(operand).LeftShift(operand, shift, result);
            return result;
        }

        public NArray<int> RightShift(NArray<int> operand, int shift)
        {
            var result = NewNArrayLike(operand);
            Provider(operand).RightShift(operand, shift, result);
            return result;
        }

        public NArrayBool RelativeOperation(NArray operand1, NArray operand2, RelativeOperator op)
        {
            if (operand1.IsScalar) throw new ArgumentException();
            var result = NArrayFactory.CreateLike<bool, double>(operand1) as NArrayBool;
            if (operand2.IsScalar)
            {
                Provider(operand1, operand2).RelativeOperation(operand1, operand2.First(), result, op);
            }
            else
            {
                Provider(operand1, operand2).RelativeOperation(operand1, operand2, result, op);
            }
            return result;
        }

        public double DotProduct(NArray operand1, NArray operand2)
        {
            return Provider(operand1, operand2).Dot(operand1, operand2);
        }

        public void MatrixMultiply(NArray operand1, NArray operand2, NArray result)
        {
            Provider(operand1, operand2).MatrixMultiply(operand1, operand2, result);
        }

        #endregion

        #region Unary Operations

        public IDisposable CreateRandomNumberStream(StorageLocation location, RandomNumberGeneratorType type, int seed)
        {
            return Provider(location).CreateRandomNumberStream(type, seed);
        }

        public void FillRandom(ContinuousDistribution distribution, NArray operand)
        {
            Provider(operand).FillRandom(distribution, operand);
        }

        public NArray<T> Index<T>(NArray<T> operand, NArrayInt indices)
        {
            var result = NewNArrayLike<T, int>(indices);
            Provider(operand).Index<T>(operand, indices, result);
            return result;
        }

        public void CholeskyDecomposition(NArray operand)
        {
            Provider(operand).CholeskyDecomposition(operand);
        }

        public void EigenvalueDecomposition(NArray operand, NArray eigenvectors, NArray eigenvalues)
        {
            Provider(operand, eigenvectors, eigenvalues).EigenvalueDecomposition(operand, eigenvectors, eigenvalues);
        }

        public void SortInPlace(NArray operand)
        {
            Provider(operand).SortInPlace(operand);
        }

        #endregion
        #region Reduction

        public double Sum(NArray operand)
        {
            return Provider(operand).Sum(operand);
        }

        #endregion
    }
}
