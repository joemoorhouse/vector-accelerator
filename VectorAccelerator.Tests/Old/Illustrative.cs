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

            var watch = new System.Diagnostics.Stopwatch(); watch.Start();
            using (var randomStream = new RandomNumberStream(location, RandomNumberGeneratorType.MRG32K3A, 111))
            {
                var normal = new Normal(randomStream, 0, 1);
                var test = NArray.CreateRandom(1000, normal);
                for (int i = 0; i < 363 * 100; ++i)
                {
                    test.FillRandom(normal);
                }
            }
            watch.Stop();
            var elapsed = watch.ElapsedMilliseconds * 5000 / (100 * 1000);

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
                return a * b + a * NMath.Exp(2 * b);
            }, expressionsDiff, a, b);

            var output = expressionsDiff.ToString();

            var expressionsDiff2 = new StringBuilder();
            
            var s = NMath.Exp(a) + 5;
            var resultDiff2 = NArray.Evaluate(() =>
            {
                return Finance.BlackScholes(CallPut.Call, s, 5, 1, 1);
                //return NMath.Exp(NMath.Sqrt(a));
            }, expressionsDiff2, s);

            var check = (Finance.BlackScholes(CallPut.Call, s + 1e-6, 5, 1, 1) - Finance.BlackScholes(CallPut.Call, s, 5, 1, 1)) / 1e-6;
            var expected = check.DebugDataView.Take(10).ToArray();
            var obtained = resultDiff2[1].DebugDataView.Take(10).ToArray();

            var output2 = expressionsDiff2.ToString();
            Console.Write(output2);
        }
    }
}
