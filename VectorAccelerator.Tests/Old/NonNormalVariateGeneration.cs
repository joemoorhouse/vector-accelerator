using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorAccelerator.Tests
{
    public class NonNormalVariateGeneration
    {
        public NArray _weights;
        public NArray _uncorrelatedVariates;
        public NArray _correlatedGaussianVariates;
        
        public void FillRandom(NArray values)
        {
            NMath.MatrixMultiply(_weights, _uncorrelatedVariates, _correlatedGaussianVariates);

            // extract a vector and interpolate
            //var vector = _correlatedGaussianVariates.


        }
    }
}
