using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using VectorAccelerator;
using VectorAccelerator.Distributions;
using VectorAccelerator.LinearAlgebraProviders;
using VectorAccelerator.DeferredExecution;
using VectorAccelerator.DeferredExecution.Expressions;

namespace VectorAccelerator
{
    /// <summary>
    /// One executor is created for each execution scope. 
    /// If the operations can be deferred for later, more efficient, execution, this is done. Otherwise
    /// the ImmediateExecutor is used.
    /// </summary>
    public class DeferringExecutor : BaseExecutor, IExecutor
    {
        BlockExpressionBuilder _builder = new BlockExpressionBuilder();

        public int VectorsLength { get { return _builder.VectorLength; } }

        public void Execute(VectorExecutionOptions options)
        {
            var codeBuilder = new CUDACodeBuilder();
            var block = _builder.ToBlock();
            codeBuilder.GenerateCode(block);
        }

        public void Evaluate(VectorExecutionOptions options, 
            out NArray[] outputs,
            NArray dependentVariable, params NArray[] independentVariables)
        {
            var dependentVariableExpression = _builder.GetParameter<double>(dependentVariable);
            var independentVariableExpressions = independentVariables.Select(v => _builder.GetParameter<double>(v)).ToArray();
            
            // differentiate, extending the block as necessary
            IList<Expression> derivativeExpressions;
            AlgorithmicDifferentiator.Differentiate(_builder, out derivativeExpressions,
                dependentVariableExpression, independentVariableExpressions);
            
            // the derivativeExpressions can be either constant expressions (a common case!) or locals
            var outputArrays = new NArray[independentVariables.Length + 1];
            outputArrays[0] = dependentVariable;

            // for locals that we know need to be peristed, we will use full storage (rather than re-assign)
            outputs = new NArray[] { dependentVariable }.Concat
                (derivativeExpressions.Select(e => (e is ConstantExpression<double>) ?
                    NArray.CreateScalar((e as ConstantExpression<double>).Value) :
                    (e as ReferencingVectorParameterExpression<double>).Array as NArray))
                    .ToArray();

            // run
            VectorBlockExpressionRunner.RunNonCompiling(_builder, Provider(StorageLocation.Host), 
                new VectorExecutionOptions(), outputs);
        }

        #region Deferrable Operations

        public override NArray<T> NewNArrayLike<T>(NArray<T> array)
        {
            return CreateLocalLike<T, T>(array) as NArray<T>;
        }

        public override NArray<S> NewNArrayLike<S, T>(NArray<T> array)
        {
            return CreateLocalLike<S, T>(array) as NArray<S>;
        }

        public void Assign<T>(NArray<T> result, NArray<T> a)
        {
            _builder.AddAssign(result, a);
        }

        public override void DoUnaryElementWiseOperation<T>(NArray<T> a, NArray<T> result, UnaryElementWiseOperation operation)
        {
            _builder.AddUnaryElementWiseOperation(a, result, operation);
        }

        public override void DoScaleOffset<T>(NArray<T> a, T scale, T offset, NArray<T> result)
        {
            _builder.AddScaleOffsetOperation<T>(a, scale, offset, result);
        }

        public override void DoBinaryElementWiseOperation<T>(NArray<T> a, NArray<T> b, NArray<T> result, ExpressionType operation)
        {
            _builder.AddBinaryElementWiseOperation(a, b, result, operation);
        }

        #endregion

        public void Assign<T>(NArray<T> operand1, Func<NArray<T>> operand2, Func<NArrayBool> condition)
        {
            // simple assignment: we evaluate the functions to generate full vectors and assign a portion.
            throw new NotImplementedException();
            //_vectorOperations.Add(new AssignOperation<T>(operand1, operand2()));
        }

        public T GetValue<T>(NArray<T> array, int index)
        {
            throw new NotImplementedException();
        }

        public NArrayBool LogicalOperation(NArrayBool operand1, NArrayBool operand2, LogicalBinaryElementWiseOperation op)
        {
            throw new NotImplementedException();
        }

        public NArrayInt ConstantLike<T>(int constantValue, NArray<T> array)
        {
            throw new NotImplementedException();
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

        public double DotProduct(NArray operand1, NArray operand2)
        {
            throw new NotImplementedException();
        }

        public double Sum(NArray operand)
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

        public IDisposable CreateRandomNumberStream(StorageLocation location, RandomNumberGeneratorType type, int seed)
        {
            throw new NotImplementedException();
        }

        public void FillRandom(ContinuousDistribution distribution, NArray operand)
        {

        }

        public NArray<T> Index<T>(NArray<T> operand, NArrayInt indices)
        {
            if (_builder.VectorLength == -1) _builder.VectorLength = indices.Length;
            if (indices.Length != _builder.VectorLength)
                throw new ArgumentException("length mismatch", "array");

            return NewNArrayLike<T>(operand);
        }

        public void CholeskyDecomposition(NArray operand)
        {
            throw new NotImplementedException();
        }

        public void EigenvalueDecomposition(NArray operand, NArray eigenvectors, NArray eigenvalues)
        {
            throw new NotImplementedException();
        }

        public void SortInPlace(NArray operand)
        {
            throw new NotImplementedException();
        }

        #endregion

        private ILocalNArray CreateLocalLike<S, T>(NArray<T> array)
        {
            if (_builder.VectorLength == -1) _builder.VectorLength = array.Length;
            if (array.Length != _builder.VectorLength)
                throw new ArgumentException("length mismatch", "array");

            return _builder.CreateLocal<T>();
        }
    }
}
