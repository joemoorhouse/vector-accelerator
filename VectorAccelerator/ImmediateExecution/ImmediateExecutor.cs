using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator.LinearAlgebraProviders;

namespace VectorAccelerator
{
    public enum ExecutionMode { Instant, Deferred }

    public class ImmediateExecutor : IExecutor
    {
        ILinearAlgebraProvider _provider = new IntelMKLLinearAlgebraProvider();
        
        public void Assign(NArray operand1, NArray operand2)
        {
            operand1 = operand2;
        }

        #region Binary Operations

        public NArray ElementWiseAdd(NArray operand1, NArray operand2)
        {
            var result = _provider.CreateLike(operand1);
            _provider.Add(operand1, operand2, result);
            return result;
        }

        public NArray ElementWiseAdd(NArray operand1, double operand2)
        {
            var result = _provider.CreateLike(operand1);
            _provider.ScaleOffset(operand1, 1, operand2, result);
            return result;
        }

        public NArray ElementWiseAdd(double operand1, NArray operand2)
        {
            var result = _provider.CreateLike(operand2);
            _provider.ScaleOffset(operand2, 1, operand1, result);
            return result;
        }

        public NArray ElementWiseSubtract(NArray operand1, NArray operand2)
        {
            var result = _provider.CreateLike(operand1);
            _provider.Subtract(operand1, operand2, result);
            return result;
        }

        public NArray ElementWiseSubtract(NArray operand1, double operand2)
        {
            var result = _provider.CreateLike(operand1);
            _provider.ScaleOffset(operand1, 1, -operand2, result);
            return result;
        }

        public NArray ElementWiseSubtract(double operand1, NArray operand2)
        {
            var result = _provider.CreateLike(operand2);
            _provider.ScaleOffset(operand2, -1, operand1, result);
            return result;
        }

        public NArray ElementWiseMultiply(NArray operand1, NArray operand2)
        {
            var result = _provider.CreateLike(operand1);
            _provider.Multiply(operand1, operand2, result);
            return result;
        }

        public NArray ElementWiseMultiply(NArray operand1, double operand2)
        {
            var result = _provider.CreateLike(operand1);
            _provider.ScaleOffset(operand1, operand2, 0, result);
            return result;
        }

        public NArray ElementWiseMultiply(double operand1, NArray operand2)
        {
            var result = _provider.CreateLike(operand2);
            _provider.ScaleOffset(operand2, operand1, 0, result);
            return result;
        }

        public NArray ElementWiseDivide(NArray operand1, NArray operand2)
        {
            var result = _provider.CreateLike(operand1);
            _provider.Divide(operand1, operand2, result);
            return result;
        }

        public NArray ElementWiseDivide(NArray operand1, double operand2)
        {
            var result = _provider.CreateLike(operand1);
            _provider.ScaleOffset(operand1, 1.0 / operand2, 0, result);
            return result;
        }

        public NArray ElementWiseDivide(double operand1, NArray operand2)
        {
            var result = _provider.CreateLike(operand2);
            _provider.ScaleOffset(operand2, 1.0 / operand1, 0, result);
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

        #endregion
    }
}
