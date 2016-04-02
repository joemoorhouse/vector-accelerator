using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator.DeferredExecution
{
    /// <summary>
    /// Unary operation that scales and offsets the operand to obtain a new vector.
    /// </summary>
    public class ScaleOffsetOperation<T> : UnaryVectorOperation<T>
    {        
        public readonly T Scale;
        public readonly T Offset;

        public ScaleOffsetOperation(NArray<T> operand, NArray<T> result, T scale, T offset,
            Action<NArray<T>, T, T, NArray<T>> scaleOffsetOperation)
            : base(operand, result, (op, res) => scaleOffsetOperation(op, scale, offset, res))
        {
            Scale = scale;
            Offset = offset;
        }

        public override bool IsVectorOperation
        {
            get { return true; }
        }

        public ScaleOffsetOperation(NArray<T> operand, NArray<T> result, T scale, T offset,
            Action<NArray<T>, NArray<T>> scaleOffsetOperation)
            : base(operand, result, scaleOffsetOperation)
        {
            Scale = scale;
            Offset = offset;
        }

        public override string ToString()
        {
            return string.Join(" ", Result.ToString(), "=", Scale.ToString(), "*", Operand.ToString(), "+", Offset.ToString());
        }

        public override NArrayOperation<T> Clone(Func<NArray<T>, NArray<T>> transform)
        {
            return new ScaleOffsetOperation<T>(transform(Operand), transform(Result), Scale, Offset, Operation);
        }
    }
}
