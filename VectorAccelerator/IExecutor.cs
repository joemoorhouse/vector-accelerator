using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorAccelerator
{    
    public interface IExecutor
    {
        void Assign(NArray operand1, NArray operand2);

        #region Binary

        NArray ElementWiseAdd(NArray operand1, NArray operand2);

        NArray ElementWiseSubtract(NArray operand1, NArray operand2);

        NArray ElementWiseMultiply(NArray operand1, NArray operand2);

        NArray ElementWiseDivide(NArray operand1, NArray operand2);

        #endregion

        #region Unary

        NArray ElementWiseNegate(NArray operand);

        NArray ElementWiseExp(NArray operand);

        NArray ElementWiseLog(NArray operand);

        NArray ElementWiseSquareRoot(NArray operand);

        NArray ElementWiseInverseSquareRoot(NArray operand);

        NArray ElementWiseCumulativeNormal(NArray operand);

        NArray ElementWiseInverseCumulativeNormal(NArray operand);

        //NArray FillRandom()

        //NArray Index(NArray<int> operand);

        #endregion
    }
}
