using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

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
            var factory = new NArrayFactory(StorageLocation.Host);

            NArray x = factory.CreateFromEnumerable(Enumerable.Range(0, length).Select(i => (double)i / length));

            var y = x > 0.5;

            //x[y] = 6;

            //var subset = x[y];

            //var y = x[x > 0.5];
        }

        public int[] BinarySearch(double[] x, double[] y, double[] xValues)
        {
            var result = new int[xValues.Length];

            for (int i = 0; i < xValues.Length; ++i)
            {
                double value = xValues[i];
                int left = 0;
                int right = x.Length - 1;
                int mid = (left + right) >> 1;
                while ((right - left) > 1)
                {
                    if (x[mid] >= value)
                        right = mid;
                    else
                        left = mid;
                    mid = (left + right) >> 1;
                }
                result[i] = left;
            }
            return result;
        }

        public void VectorBinarySearch()
        {
            var factory = new NArrayFactory(StorageLocation.Host);
            
            int length = 30000;
            int knotPointsLength = 100;
            var valuesArray = Enumerable.Range(0, length).Select(i => (double)i / length + 0.5).ToArray();
            var values = factory.CreateFromEnumerable(valuesArray);
            var xArray = Enumerable.Range(0, knotPointsLength).Select(i => (double)i / knotPointsLength).ToArray();
            var yArray = Enumerable.Range(0, knotPointsLength).Select(i => 5 + (double)i / knotPointsLength).ToArray();
            NArray x = factory.CreateFromEnumerable(xArray);
            NArray y = factory.CreateFromEnumerable(yArray);

            var watch = new Stopwatch(); watch.Start();

            for (int i = 0; i < 50000; ++i)
            {
                BinarySearch(xArray, yArray, valuesArray);
            }

            Console.WriteLine("Baseline sequential");
            Console.WriteLine(watch.ElapsedMilliseconds); watch.Restart();

            for (int i = 0; i < 100; ++i)
            {
                var left = NArrayFactory.CreateConstantLike(values, 0);
                var right = NArrayFactory.CreateConstantLike(values, x.Length - 1);
                var mid = (left + right) >> 1;

                //// we are going to stop when right - left = 1
                //// for the vector version, we cannot allow the break condition to be different for different
                //// vector elements

                int elements = (x.Length - 1) << 1;
                while (elements > 1)
                {
                    var active = (right - left) > 1;
                    var midValue = x[mid];

                    right[midValue >= values && active] = mid;

                    // or alternate form:
                    //right.Assign(
                    //    () => midValue >= values && active,
                    //    () => mid);
                    // A conditional in a CUDA kernel would cause the evaluation of both branches 
                    // But we can be more efficient on CPU (e.g. mask vector indexing)

                    left[midValue < values && active] = mid;
                    mid = left + right >> 1;
                    elements = elements >> 1;
                }
            }

            Console.WriteLine("Vector sequential");
            Console.WriteLine(watch.ElapsedMilliseconds); watch.Restart();

            //check!
            //(right - left).DebugDataView.ToArray()
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
