using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator.DeferredExecution
{
    /// <summary>
    /// Unary operation that scales and offsets the operand to obtain a new vector.
    /// </summary>
    public class ScaleOffsetOperation : UnaryVectorOperation
    {
        public readonly double Scale;
        public readonly double Offset;

        public ScaleOffsetOperation(NArray operand, NArray result, double scale, double offset,
            Action<NArray, double, double, NArray> scaleOffsetOperation)
            : base(operand, result, (op, res) => scaleOffsetOperation(op, scale, offset, res))
        {
            Scale = scale;
            Offset = offset;
        }

        public override bool IsVectorOperation
        {
            get { return true; }
        }

        public ScaleOffsetOperation(NArray operand, NArray result, double scale, double offset,
            Action<NArray, NArray> scaleOffsetOperation)
            : base(operand, result, scaleOffsetOperation)
        {
            Scale = scale;
            Offset = offset;
        }

        public override string ToString()
        {
            return string.Join(" ", Result.ToString(), "=", Scale.ToString(), "*", Operand.ToString(), "+", Offset.ToString());
        }

        public override NArrayOperation Clone(Func<NArray, NArray> transform)
        {
            return new ScaleOffsetOperation(transform(Operand), transform(Result), Scale, Offset, Operation);
        }
    }
}
