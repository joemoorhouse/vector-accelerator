using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VectorAccelerator;
using VectorAccelerator.Distributions;
using VectorAccelerator.LinearAlgebraProviders;

namespace VectorAccelerator.DeferredExecution
{
    /// <summary>
    /// One executor is created for each execution scope. 
    /// All operations within the scope are executed using vector primitives but with efficient use of
    /// temporary memory.
    /// </summary>
    public class DeferredPrimitivesExecutor : BaseExecutor, IExecutor
    {
        ILinearAlgebraProvider _provider = new IntelMKLLinearAlgebraProvider();
        public override ILinearAlgebraProvider Provider
        {
            get { return _provider; }
        }
        
        List<NArrayOperation> _vectorOperations = new List<NArrayOperation>();
        List<ILocalNArray> _localVariables = new List<ILocalNArray>();
        int _vectorLength = -1;

        public int VectorsLength { get { return _vectorLength; } }

        public List<ILocalNArray> LocalVariables { get { return _localVariables; } }

        public List<NArrayOperation> VectorOperations { get { return _vectorOperations; } }

        public string DebugString()
        {
            var builder = new StringBuilder();
            VectorOperations.ForEach(o => builder.Append(o.ToString() + Environment.NewLine));
            return builder.ToString();
        }

        public void Execute(VectorExecutionOptions options)
        {
            VectorOperationRunner.Execute(this, _provider, options);
        }

        public override NArray<T> NewNArrayLike<T>(NArray<T> array)
        {
            return CreateLocalLike<T, T>(array) as NArray<T>;
        }

        public override NArray<S> NewNArrayLike<S, T>(NArray<T> array)
        {
            return CreateLocalLike<S, T>(array) as NArray<S>;
        }

        public override void DoScaleOffset<T>(NArray<T> a, T scale, T offset, NArray<T> result)
        {
            _vectorOperations.Add(
                    new ScaleOffsetOperation<T>(a, result, scale, offset, ElementWise<T>().ScaleOffset));
        }

        public override void DoBinaryElementWiseOperation<T>(NArray<T> a, NArray<T> b, NArray<T> result, BinaryElementWiseOperation operation)
        {
            _vectorOperations.Add(new BinaryVectorOperation<T>(a, b, result, 
                (op1, op2, r) => ElementWise<T>().BinaryElementWiseOperation(op1, op2, r, operation)));
        }

        public NArrayBool LogicalOperation(NArrayBool operand1, NArrayBool operand2, LogicalBinaryElementWiseOperation op)
        {
            throw new NotImplementedException();
        }

        public override void DoUnaryElementWiseOperation<T>(NArray<T> a, NArray<T> result, UnaryElementWiseOperation operation)
        {
            _vectorOperations.Add(new UnaryVectorOperation<T>(a, result,
                (op1, r) => ElementWise<T>().UnaryElementWiseOperation(op1, r, operation)));
        }

        public void Assign<T>(NArray<T> operand1, NArray<T> operand2)
        {
            _vectorOperations.Add(new AssignOperation<T>(operand1, operand2));
        }

        public void Assign<T>(NArray<T> operand1, Func<NArray<T>> operand2, Func<NArrayBool> condition)
        {
            // simple assignment: we evaluate the functions to generate full vectors and assign a portion.
            throw new NotImplementedException();
            //_vectorOperations.Add(new AssignOperation<T>(operand1, operand2()));
        }

        #region Creation

        public NArrayInt ConstantLike<T>(int constantValue, NArray<T> array)
        {
            throw new NotImplementedException();
        }

        #endregion

        private ILocalNArray CreateLocalLike<S, T>(NArray<T> array)
        {
            if (_vectorLength == -1) _vectorLength = array.Length;
            if (array.Length != _vectorLength)
                throw new ArgumentException("length mismatch", "array");

            return CreateLocalOfLength<S>(_vectorLength);
        }

        private ILocalNArray CreateLocalOfLength<T>(int length)
        {
            ILocalNArray localArray = null;
            if (typeof(T) == typeof(double)) localArray = new LocalNArray(_localVariables.Count, length);
            else if (typeof(T) == typeof(int)) localArray = new LocalNArrayInt(_localVariables.Count, length);
            _localVariables.Add(localArray);
            return localArray;
        }

        #region Binary Operations

        public void LeftShift(NArray<int> a, int shift, NArray<int> result)
        {
            throw new NotImplementedException();
        }

        public void RightShift(NArray<int> a, int shift, NArray<int> result)
        {
            throw new NotImplementedException();
        }

        public NArrayBool RelativeOperation(NArray operand1, NArray operand2, RelativeOperator op)
        {
            throw new NotImplementedException();
        }

        public void MatrixMultiply(NArray operand1, NArray operand2, NArray result)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Unary Operations

        public void Add(NArray operand1, NArray operand2)
        {
            if (operand1.Length != operand2.Length) throw new ArgumentException("length mismatch");
           
            //_vectorOperations.Add(new BinaryVectorOperation(operand1, operand2, operand1, _provider.Add));
        }

        public NArray<int> LeftShift(NArray<int> operand, int shift)
        {
            throw new NotImplementedException();
        }

        public NArray<int> RightShift(NArray<int> operand, int shift)
        {
            throw new NotImplementedException();
        }

        public IDisposable CreateRandomNumberStream(RandomNumberGeneratorType type, int seed)
        {
            throw new NotImplementedException();
        }

        public void FillRandom(ContinuousDistribution distribution, NArray operand)
        {

        }

        public NArray<T> Index<T>(NArray<T> operand, NArrayInt indices)
        {
            if (_vectorLength == -1) _vectorLength = indices.Length;
            if (indices.Length != _vectorLength)
                throw new ArgumentException("length mismatch", "array");

            return NewNArrayLike<T>(operand);
        }

        #endregion
    }
}
