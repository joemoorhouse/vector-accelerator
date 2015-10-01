using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VectorAccelerator.Distributions;

namespace VectorAccelerator.LinearAlgebraProviders
{
    public interface ILinearAlgebraProvider
    {
        void Add(NArray a, NArray b, NArray result);

        void Subtract(NArray a, NArray b, NArray result);

        void Multiply(NArray a, NArray b, NArray result);

        void Divide(NArray a, NArray b, NArray result);

        void Inverse(NArray a, NArray result);

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

        /// <summary>
        /// Matrix multiply: c = a * b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        void MatrixMultiply(NArray a, NArray b, NArray c);

        NArray CreateLike(NArray a);

        IDisposable CreateRandomNumberStream(RandomNumberGeneratorType type, int seed);

        void FillRandom(ContinuousDistribution distribution, NArray values);
    }
}
