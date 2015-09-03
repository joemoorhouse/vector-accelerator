using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator.LinearAlgebraProviders
{
    public interface ILinearAlgebraProvider
    {
        void Add(NArray a, NArray b, NArray result);

        void Subtract(NArray a, NArray b, NArray result);

        void Multiply(NArray a, NArray b, NArray result);

        void Divide(NArray a, NArray b, NArray result);

        void Exp(NArray a, NArray result);

        void Log(NArray a, NArray result);

        void SquareRoot(NArray a, NArray result);

        void InverseSquareRoot(NArray a, NArray result);

        void CumulativeNormal(NArray a, NArray result);

        void InverseCumulativeNormal(NArray a, NArray result);

        /// <summary>
        /// Scales and offsets a vector by constant amounts
        /// </summary>
        /// <param name="a">Input vector</param>
        /// <param name="scale">Scaling factor</param>
        /// <param name="offset">Offset</param>
        /// <returns></returns>
        void ScaleOffset(NArray a, double scale, double offset, NArray result);

        NArray CreateLike(NArray a);
    }
}
