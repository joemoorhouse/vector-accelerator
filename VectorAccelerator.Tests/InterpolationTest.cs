using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorAccelerator.Tests
{
    public class InterpolationTest
    {
        public void NonNormalVariates()
        {
            // we would calculate a batch of random variates for 10,000 factors, say 5000
            // 10,000 * 5,000 * 4 = 200 MB
            // end up with vectors of variates. Put each one through interpolation.
        }
        
        public void StandardFormInterpolateTest()
        {
            // knot-points:
            NArray _c0, _c1; // 100 elements say
            NArray _x; // 100 elements say
            
            NArray t = null; // 10000 elements say: the requested points

            // performance-critical: a form of binary search
            var k = LeftSegmentIndex(t); // vector of 10000 segments

            // performance-critical (for GPU, would actually want _c0 and _c1 in shared memory)
            //var result = _c0[k] + (t - _x[k]) * _c1[k];

            // _co[k] is a vector of 10000 elements obtained by indexing _c0 10000 times
            // a cubic spline would look like:
            //_c0[k] + x * (_c1[k] + x * (_c2[k] + x * _c3[k]));

        }

        public NArray LeftSegmentIndex(NArray t)
        {
            return null;
        }
    }
}
