using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NArray
{
    internal interface INArrayOperationExecutor
    {
        NArray ElementWiseAdd(NArray left, NArray right);

        NArray ElementWiseSubtract(NArray left, NArray right);

        NArray ElementWiseMultiply(NArray left, NArray right);

        NArray ElementWiseDivide(NArray left, NArray right);

        void InPlaceAdd(NArray result, NArray left, NArray right);

        void InPlaceMultiply(NArray result, NArray left, NArray right);

        NArray UnaryElementWiseOperation(NArray operand, UnaryElementWiseOperations operation);
    }
}
