using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator.LinearAlgebraProviders
{
    public interface IImmediateLinearAlgebraProvider
    {
        NAray Add(NAray a, NAray b);

        NAray Subtract(NAray a, NAray b);

        NAray Multiply(NAray a, NAray b);

        NAray Divide(NAray a, NAray b);

        /// <summary>
        /// Scales and offsets a vector by constant amounts
        /// </summary>
        /// <param name="a">Input vector</param>
        /// <param name="scale">Scaling factor</param>
        /// <param name="offset">Offset</param>
        /// <returns></returns>
        NAray ScaleOffset(NAray a, double scale, double offset);
    }
}
