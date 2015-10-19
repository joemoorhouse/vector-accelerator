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
        ILinearAlgebraProvider _provider = new IntelMKLLinearAlgebraProvider();
        public override ILinearAlgebraProvider Provider
        {
            get { return _provider; }
        }
        
        public void Assign<T>(NArray<T> operand1, NArray<T> operand2)
        {
            var managedStorage = operand2.Storage as ManagedStorage<T>;
            operand1.Storage = new ManagedStorage<T>((T[])managedStorage.Array.Clone(), 
                managedStorage.ArrayStart, managedStorage.Length);
        }

        public void Assign<T>(NArray<T> operand1, Func<NArray<T>> operand2, Func<NArrayBool> condition)
        {
            // simple assignment: we evaluate the functions to generate full vectors and assign a portion.
            _provider.Assign(operand2(), condition(), operand1);
        }

        public override NArray<T> NewNArrayLike<T>(NArray<T> array)
        {
            return _provider.CreateLike<T, T>(array);
        }

        public override NArray<S> NewNArrayLike<S, T>(NArray<T> array)
        {
            return _provider.CreateLike<S, T>(array);
        }

        public override void DoScaleOffset<T>(NArray<T> a, T scale, T offset, NArray<T> result)
        {
            ElementWise<T>().ScaleOffset(a, scale, offset, result);
        }

        public override void DoBinaryElementWiseOperation<T>(NArray<T> a, NArray<T> b, NArray<T> result, BinaryElementWiseOperation operation)
        {
            ElementWise<T>().BinaryElementWiseOperation(a, b, result, operation);
        }

        public NArrayBool LogicalOperation(NArrayBool operand1, NArrayBool operand2, LogicalBinaryElementWiseOperation op)
        {
            var result = NewNArrayLike(operand1) as NArrayBool;
            _provider.LogicalOperation(operand1, operand2, result, op);
            return result;
        }

        public override void DoUnaryElementWiseOperation<T>(NArray<T> a, NArray<T> result, UnaryElementWiseOperation operation)
        {
            ElementWise<T>().UnaryElementWiseOperation(a, result, operation);
        }

        #region Creation

        public NArrayInt ConstantLike<T>(int constantValue, NArray<T> array)
        {
            return _provider.CreateConstantLike(array, constantValue) as NArrayInt;
        }

        #endregion

        #region Binary Operations

        public void Add(NArray operand1, NArray operand2)
        {
            _provider.BinaryElementWiseOperation(operand1, operand2, operand1, BinaryElementWiseOperation.Add);
        }

        public NArray<int> LeftShift(NArray<int> operand, int shift)
        {
            var result = NewNArrayLike(operand);
            _provider.LeftShift(operand, shift, result);
            return result;
        }

        public NArray<int> RightShift(NArray<int> operand, int shift)
        {
            var result = NewNArrayLike(operand);
            _provider.RightShift(operand, shift, result);
            return result;
        }

        public NArrayBool RelativeOperation(NArray operand1, NArray operand2, RelativeOperator op)
        {
            if (operand1.IsScalar) throw new ArgumentException();
            var result = _provider.CreateLike<bool, double>(operand1) as NArrayBool;
            if (operand2.IsScalar)
            {
                _provider.RelativeOperation(operand1, operand2.First(), result, op);
            }
            else
            {
                _provider.RelativeOperation(operand1, operand2, result, op);
            }
            return result;
        }

        public void MatrixMultiply(NArray operand1, NArray operand2, NArray result)
        {
            _provider.MatrixMultiply(operand1, operand2, result);
        }

        #endregion

        #region Unary Operations

        public IDisposable CreateRandomNumberStream(RandomNumberGeneratorType type, int seed)
        {
            return _provider.CreateRandomNumberStream(type, seed);
        }

        public void FillRandom(ContinuousDistribution distribution, NArray operand)
        {
            _provider.FillRandom(distribution, operand);
        }

        public NArray<T> Index<T>(NArray<T> operand, NArrayInt indices)
        {
            var result = NewNArrayLike(operand);
            _provider.Index<T>(operand, indices, result);
            return result;
        }

        #endregion
    }
}
