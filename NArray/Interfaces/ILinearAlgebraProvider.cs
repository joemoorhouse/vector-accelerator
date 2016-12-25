using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NArray.DeferredExecution.Expressions;

namespace NArray
{
    public enum UnaryElementWiseOperations { ScaleInverse, Negate, Exp, Log, SquareRoot, Inverse, InverseSquareRoot, Normal, CumulativeNormal, InverseCumulativeNormal, ScaleOffset };

    internal interface ILinearAlgebraProvider
    {
        void ScaleOffset(INArrayStorage operand, double scale, double offset, INArrayStorage result);
        void ScaleInverse(INArrayStorage operand, double scale, INArrayStorage result);
        void BinaryElementWiseOperation(INArrayStorage left, INArrayStorage right, ExpressionType operation, INArrayStorage result);
        void UnaryElementWiseOperation(INArrayStorage operand, INArrayStorage result, UnaryElementWiseOperations operation);
    }
}
