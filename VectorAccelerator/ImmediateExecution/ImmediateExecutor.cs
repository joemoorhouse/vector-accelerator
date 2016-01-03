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

        public override void DoScaleOffset<T>(NArray<T> a, T scale, T offset, NArray<T> result)
        {
            ElementWise<T>(a).ScaleOffset(a, scale, offset, result);
        }

        public override void DoBinaryElementWiseOperation<T>(NArray<T> a, NArray<T> b, NArray<T> result, BinaryElementWiseOperation operation)
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

        public void Add(NArray operand1, NArray operand2)
        {
            Provider(operand1, operand2).BinaryElementWiseOperation(operand1, operand2, operand1, BinaryElementWiseOperation.Add);
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
