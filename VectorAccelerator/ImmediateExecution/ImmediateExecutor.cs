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
        IImmediateLinearAlgebraProvider _provider = new IntelMKLLinearAlgebraProvider();
        
        public void Assign(NAray operand1, NAray operand2)
        {
            throw new NotImplementedException();
        }

        #region Binary Operations

        public NAray ElementWiseAdd(NAray operand1, NAray operand2)
        {
            return _provider.Add(operand1, operand2);
        }

        public NAray ElementWiseAdd(NAray operand1, double operand2)
        {
            return _provider.ScaleOffset(operand1, 1, operand2);
        }

        public NAray ElementWiseAdd(double operand1, NAray operand2)
        {
            return _provider.ScaleOffset(operand2, 1, operand1);
        }

        public NAray ElementWiseSubtract(NAray operand1, NAray operand2)
        {
            return _provider.Subtract(operand1, operand2);
        }

        public NAray ElementWiseSubtract(NAray operand1, double operand2)
        {
            return _provider.ScaleOffset(operand1, 1, -operand2);
        }

        public NAray ElementWiseSubtract(double operand1, NAray operand2)
        {
            return _provider.ScaleOffset(operand2, -1, operand1);
        }

        public NAray ElementWiseMultiply(NAray operand1, NAray operand2)
        {
            return _provider.Multiply(operand1, operand2);
        }

        public NAray ElementWiseMultiply(NAray operand1, double operand2)
        {
            return _provider.ScaleOffset(operand1, operand2, 0);
        }

        public NAray ElementWiseMultiply(double operand1, NAray operand2)
        {
            return _provider.ScaleOffset(operand2, operand1, 0);
        }

        public NAray ElementWiseDivide(NAray operand1, NAray operand2)
        {
            return _provider.Divide(operand1, operand2);
        }

        public NAray ElementWiseDivide(NAray operand1, double operand2)
        {
            return _provider.ScaleOffset(operand1, 1.0 / operand2, 0);
        }

        public NAray ElementWiseDivide(double operand1, NAray operand2)
        {
            return _provider.ScaleOffset(operand2, 1.0 / operand1, 0);
        }

        #endregion

        #region Unary Operations

        public NAray ElementWiseExp(NAray operand)
        {
            throw new NotImplementedException();
        }

        public NAray ElementWiseLog(NAray operand)
        {
            throw new NotImplementedException();
        }

        public NAray ElementWiseNegate(NAray operand)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
