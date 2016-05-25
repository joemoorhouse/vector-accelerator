using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator;
using VectorAccelerator.NArrayStorage;
using VectorAccelerator.Distributions;
using VectorAccelerator.LinearAlgebraProviders;

namespace VectorAccelerator.Tests
{
    public class Illustrative
    {
        public void Example1()
        {
            var location = StorageLocation.Host;

            NArray a, b;
            using (var randomStream = new RandomNumberStream(location, RandomNumberGeneratorType.MRG32K3A, 111))
            {
                var normal = new Normal(randomStream, 0, 1);
                a = NArray.CreateRandom(5000, normal);
                b = NArray.CreateRandom(5000, normal);
            }

            var expressions = new StringBuilder();
            var result = NArray.Evaluate(() =>
            {
                return a * b + a * NMath.Exp(b);
            }, expressions);

            var expressionsDiff = new StringBuilder();
            var resultDiff = NArray.Evaluate(() =>
            {
                return a * b + a * NMath.Exp(b);
            }, expressionsDiff, a, b);
        }
    }
}
