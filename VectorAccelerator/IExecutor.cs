using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorAccelerator
{    
    public interface IExecutor
    {
        void Assign(NAray operand1, NAray operand2);

        #region Binary

        NAray ElementWiseAdd(NAray operand1, NAray operand2);

        NAray ElementWiseAdd(NAray operand1, double operand2);

        NAray ElementWiseAdd(double operand1, NAray operand2);

        NAray ElementWiseSubtract(NAray operand1, NAray operand2);

        NAray ElementWiseSubtract(NAray operand1, double operand2);

        NAray ElementWiseSubtract(double operand1, NAray operand2);

        NAray ElementWiseMultiply(NAray operand1, NAray operand2);

        NAray ElementWiseMultiply(NAray operand1, double operand2);

        NAray ElementWiseMultiply(double operand1, NAray operand2);

        NAray ElementWiseDivide(NAray operand1, NAray operand2);

        NAray ElementWiseDivide(NAray operand1, double operand2);

        NAray ElementWiseDivide(double operand1, NAray operand2);

        #endregion

        #region Unary

        NAray ElementWiseNegate(NAray operand);

        NAray ElementWiseExp(NAray operand);

        NAray ElementWiseLog(NAray operand);

        #endregion
    }
}
