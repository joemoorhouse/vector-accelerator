using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VectorAccelerator.LinearAlgebraProviders;

namespace VectorAccelerator.DeferredExecution
{
    /// <summary>
    /// One executor is created for each execution scope. 
    /// All operations within the scope are executed using vector primitives but with efficient use of
    /// temporary memory.
    /// </summary>
    public class DeferredPrimitivesExecutor : IExecutor
    {
        List<VectorOperation> _vectorOperations = new List<VectorOperation>();
        ILinearAlgebraProvider _provider = new IntelMKLLinearAlgebraProvider();
        List<LocalNArray> _localVariables = new List<LocalNArray>();
        int _vectorLength = -1;

        public int VectorsLength { get { return _vectorLength; } }

        public List<LocalNArray> LocalVariables { get { return _localVariables; } }

        public List<VectorOperation> VectorOperations { get { return _vectorOperations; } }

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

        public void Assign(NArray operand1, NArray operand2)
        {
            _vectorOperations.Add(new AssignOperation(operand1, operand2));
        }

        private LocalNArray CreateLocalLike(NArray array)
        {
            if (_vectorLength == -1) _vectorLength = array.Length;
            if (array.Length != _vectorLength)
                throw new ArgumentException("length mismatch", "array");
            var localArray = new LocalNArray(_localVariables.Count, _vectorLength);
            _localVariables.Add(localArray);
            return localArray;
        }

        #region Binary Operations

        public NArray ElementWiseAdd(NArray operand1, NArray operand2)
        {
            if (operand1.IsScalar && operand2.IsScalar) return new NArray(operand1.First() + operand2.First());
            NArray result;
            if (operand1.IsScalar && !operand2.IsScalar)
            {
                result = CreateLocalLike(operand2);
                _vectorOperations.Add(
                    new ScaleOffsetOperation(operand2, result, 1, operand1.First(), _provider.ScaleOffset));
            }
            else if (!operand1.IsScalar && operand2.IsScalar)
            {
                result = CreateLocalLike(operand1);
                _vectorOperations.Add(
                    new ScaleOffsetOperation(operand1, result, 1, operand2.First(), _provider.ScaleOffset));
            }
            else
            {
                result = CreateLocalLike(operand1);
                _vectorOperations.Add(new BinaryVectorOperation(operand1, operand2, result, _provider.Add));
            }
            return result;
        }

        public NArray ElementWiseSubtract(NArray operand1, NArray operand2)
        {
            if (operand1.IsScalar && operand2.IsScalar) return new NArray(operand1.First() - operand2.First());
            NArray result;
            if (operand1.IsScalar && !operand2.IsScalar)
            {
                result = CreateLocalLike(operand2);
                _vectorOperations.Add(
                    new ScaleOffsetOperation(operand2, result, -1, operand1.First(), _provider.ScaleOffset));
            }
            else if (!operand1.IsScalar && operand2.IsScalar)
            {
                result = CreateLocalLike(operand1);
                _vectorOperations.Add(
                    new ScaleOffsetOperation(operand1, result, 1, -operand2.First(), _provider.ScaleOffset));
            }
            else
            {
                result = CreateLocalLike(operand1);
                _vectorOperations.Add(new BinaryVectorOperation(operand1, operand2, result, _provider.Subtract));
            }
            return result;
        }

        public NArray ElementWiseMultiply(NArray operand1, NArray operand2)
        {
            if (operand1.IsScalar && operand2.IsScalar) return new NArray(operand1.First() * operand2.First());
            NArray result;
            if (operand1.IsScalar && !operand2.IsScalar)
            {
                result = CreateLocalLike(operand2);
                _vectorOperations.Add(
                    new ScaleOffsetOperation(operand2, result, operand1.First(), 0, _provider.ScaleOffset));
            }
            else if (!operand1.IsScalar && operand2.IsScalar)
            {
                result = CreateLocalLike(operand1);
                _vectorOperations.Add(
                    new ScaleOffsetOperation(operand1, result, operand2.First(), 0, _provider.ScaleOffset));
            }
            else
            {
                result = CreateLocalLike(operand1);
                _vectorOperations.Add(new BinaryVectorOperation(operand1, operand2, result, _provider.Multiply));
            }
            return result;
        }

        public NArray ElementWiseDivide(NArray operand1, NArray operand2)
        {
            if (operand1.IsScalar && operand2.IsScalar) return new NArray(operand1.First() / operand2.First());
            NArray result;
            if (operand1.IsScalar && !operand2.IsScalar)
            {
                result = CreateLocalLike(operand2);
                // special case, we invert and multiply for efficiency
                // invert
                _vectorOperations.Add(new UnaryVectorOperation(operand2, result, _provider.Inverse));

                // and then multiply
                _vectorOperations.Add(
                    new ScaleOffsetOperation(operand1, result, operand1.First(), 0, _provider.ScaleOffset));
            }
            else if (!operand1.IsScalar && operand2.IsScalar)
            {
                result = CreateLocalLike(operand1);
                _vectorOperations.Add(
                    new ScaleOffsetOperation(operand1, result, 1.0 / operand2.First(), 0, _provider.ScaleOffset));

                //_vectorOperations.Add(new UnaryVectorOperation(operand1, result,
                //(a, res) => _provider.ScaleOffset(a, 1.0 / operand2.First(), 0, res)));
            }
            else
            {
                result = CreateLocalLike(operand1);
                _vectorOperations.Add(new BinaryVectorOperation(operand1, operand2, result, _provider.Divide));
            }
            return result;
        }

        #endregion

        #region Unary Operations

        public NArray ElementWiseNegate(NArray operand)
        {
            return CreateLocalLike(operand);
        }

        public NArray ElementWiseExp(NArray operand)
        {
            var result = CreateLocalLike(operand);
            _vectorOperations.Add(new UnaryVectorOperation(operand, result, _provider.Exp));
            return result;
        }

        public NArray ElementWiseLog(NArray operand)
        {
            var result = CreateLocalLike(operand);
            _vectorOperations.Add(new UnaryVectorOperation(operand, result, _provider.Log));
            return result;
        }

        public NArray ElementWiseSquareRoot(NArray operand)
        {
            var result = CreateLocalLike(operand);
            _vectorOperations.Add(new UnaryVectorOperation(operand, result, _provider.SquareRoot));
            return result;
        }

        public NArray ElementWiseInverseSquareRoot(NArray operand)
        {
            var result = CreateLocalLike(operand);
            _vectorOperations.Add(new UnaryVectorOperation(operand, result, _provider.InverseSquareRoot));
            return result;
        }

        public NArray ElementWiseCumulativeNormal(NArray operand)
        {
            var result = CreateLocalLike(operand);
            _vectorOperations.Add(new UnaryVectorOperation(operand, result, _provider.CumulativeNormal));
            return result;
        }

        public NArray ElementWiseInverseCumulativeNormal(NArray operand)
        {
            var result = CreateLocalLike(operand);
            _vectorOperations.Add(new UnaryVectorOperation(operand, result, _provider.InverseCumulativeNormal));
            return result;
        }

        #endregion
    }
}
