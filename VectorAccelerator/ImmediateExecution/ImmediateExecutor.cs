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

    public class ImmediateExecutor : IExecutor
    {
        ILinearAlgebraProvider _provider = new IntelMKLLinearAlgebraProvider();
        
        public void Assign(NArray operand1, NArray operand2)
        {
            var managedStorage = operand2.Storage as ManagedStorage<double>;
            operand1.Storage = new ManagedStorage<double>((double[])managedStorage.Array.Clone(), managedStorage.ArrayStart, managedStorage.Length);
        }

        #region Binary Operations

        public NArray ElementWiseAdd(NArray operand1, NArray operand2)
        {
            NArray result;
            if (operand1.IsScalar && !operand2.IsScalar)
            {
                result = _provider.CreateLike(operand2);
                _provider.ScaleOffset(operand2, 1, operand1.First(), result);
            }
            else if (!operand1.IsScalar && operand2.IsScalar)
            {
                result = _provider.CreateLike(operand1);
                _provider.ScaleOffset(operand1, 1, operand2.First(), result);
            }
            else 
            {
                result = _provider.CreateLike(operand1);
                _provider.Add(operand1, operand2, result);
            }
            return result;
        }

        public NArray ElementWiseSubtract(NArray operand1, NArray operand2)
        {
            NArray result;
            if (operand1.IsScalar && !operand2.IsScalar)
            {
                result = _provider.CreateLike(operand2);
                _provider.ScaleOffset(operand2, -1, operand1.First(), result);
            }
            else if (!operand1.IsScalar && operand2.IsScalar)
            {
                result = _provider.CreateLike(operand1);
                _provider.ScaleOffset(operand1, 1, -operand2.First(), result);
            }
            else
            {
                result = _provider.CreateLike(operand1);
                _provider.Subtract(operand1, operand2, result);
            }
            return result;
        }

        public NArray ElementWiseMultiply(NArray operand1, NArray operand2)
        {
            NArray result;
            if (operand1.IsScalar && !operand2.IsScalar)
            {
                result = _provider.CreateLike(operand2);
                _provider.ScaleOffset(operand2, operand1.First(), 0, result);
            }
            else if (!operand1.IsScalar && operand2.IsScalar)
            {
                result = _provider.CreateLike(operand1);
                _provider.ScaleOffset(operand1, operand2.First(), 0, result);
            }
            else
            {
                result = _provider.CreateLike(operand1);
                _provider.Multiply(operand1, operand2, result);
            }
            return result;
        }

        public NArray ElementWiseDivide(NArray operand1, NArray operand2)
        {
            NArray result;
            if (operand1.IsScalar && !operand2.IsScalar)
            {
                // special case, we invert and multiply for efficiency
                result = _provider.CreateLike(operand2);
                _provider.Inverse(operand2, result);
                _provider.ScaleOffset(result, operand1.First(), 0, result);
            }
            else if (!operand1.IsScalar && operand2.IsScalar)
            {
                result = _provider.CreateLike(operand1);
                _provider.ScaleOffset(operand1, 1.0 / operand2.First(), 0, result);
            }
            else
            {
                result = _provider.CreateLike(operand1);
                _provider.Divide(operand1, operand2, result);
            }
            return result;
        }

        #endregion

        #region Unary Operations

        public NArray ElementWiseNegate(NArray operand)
        {
            var result = _provider.CreateLike(operand);
            _provider.ScaleOffset(operand, -1.0, 0, result);
            return result;
        }

        public NArray ElementWiseExp(NArray operand)
        {
            var result = _provider.CreateLike(operand);
            _provider.Exp(operand, result);
            return result;
        }

        public NArray ElementWiseLog(NArray operand)
        {
            var result = _provider.CreateLike(operand);
            _provider.Log(operand, result);
            return result;
        }

        public NArray ElementWiseSquareRoot(NArray operand)
        {
            var result = _provider.CreateLike(operand);
            _provider.SquareRoot(operand, result);
            return result;
        }

        public NArray ElementWiseInverseSquareRoot(NArray operand)
        {
            var result = _provider.CreateLike(operand);
            _provider.InverseSquareRoot(operand, result);
            return result;
        }

        public NArray ElementWiseCumulativeNormal(NArray operand)
        {
            var result = _provider.CreateLike(operand);
            _provider.CumulativeNormal(operand, result);
            return result;
        }

        public NArray ElementWiseInverseCumulativeNormal(NArray operand)
        {
            var result = _provider.CreateLike(operand);
            _provider.InverseCumulativeNormal(operand, result);
            return result;
        }

        public void Add(NArray operand1, NArray operand2)
        {
            _provider.Add(operand1, operand2, operand1);
        }

        public IDisposable CreateRandomNumberStream(RandomNumberGeneratorType type, int seed)
        {
            return _provider.CreateRandomNumberStream(type, seed);
        }

        public void FillRandom(ContinuousDistribution distribution, NArray operand)
        {
            _provider.FillRandom(distribution, operand);
        }

        public NArray Index(NArrayInt indices)
        {
            var result = new NArray(indices.Storage.Length);
            return result;
        }

        #endregion
    }
}
