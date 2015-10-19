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
        

        public void ConditionalExample()
        {
            int length = 1000;
            NArray x = NArray.CreateFromEnumerable(Enumerable.Range(0, length).Select(i => (double)i / length));

            var y = x > 0.5;

            //x[y] = 6;

            //var subset = x[y];

            //var y = x[x > 0.5];
        }


        public void VectorBinarySearch()
        {
            int length = 1000;
            var values = NArray.CreateFromEnumerable(Enumerable.Range(0, length).Select(i => (double)i / length + 0.5));
            NArray x = NArray.CreateFromEnumerable(Enumerable.Range(0, length).Select(i => (double)i / length));
            NArray y = NArray.CreateFromEnumerable(Enumerable.Range(0, length).Select(i => 5 + (double)i / length));

            var left = NArrayInt.CreateConstantLike(0, values);
            var right = NArrayInt.CreateConstantLike(x.Length - 1, values);
            var mid = (left + right) >> 1;

            //// we are going to stop when right - left = 1
            //// for the vector version, we cannot allow the break condition to be different for different
            //// vector elements

            int elements = x.Length - 1;

            while (elements > 1)
            {
                var active = (right - left) > 1;
                var midValue = x[mid];

                right.Assign(
                    () => midValue >= values && active,
                    () => mid);
                
                // in theory we could also write: 
                //right[midValue >= values && active] = mid;
                // A conditional in a CUDA kernel would cause the evaluation of both branches 
                // But we can be more efficient on CPU (e.g. VML mask vector indexing)


                //right.Assign()

                 // asign partial
            
                //    left[midValue < value && active] = mid;
            //    mid = left + right >> 1;
            //    elements >> 1;
            }

            //while ((right - left) > 1)
            //{
            //    if (x[mid] >= value)
            //        right = mid;
            //    else
            //        left = mid;
            //    mid = (left + right) >> 1;
            //}
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
