using System;
using System.Linq;
using VectorAccelerator;
using VectorAccelerator.NArrayStorage;
using VectorAccelerator.Distributions;
using NUnit.Framework;

namespace VectorAccelerator.Tests
{
    [TestFixture]
    public class MultiplyAggregateTests
    {
        /// <summary>
        /// Test of efficiency for a large number of simple vector operations.
        /// In particular, tests enhanced recycling of locals.
        /// </summary>
        public void SimpleTest()
        {
            // create 500 random vectors
            var vectorLength = 5000;
            var vectors = new NArray[500];
            using (var randomStream = new RandomNumberStream(StorageLocation.Host, RandomNumberGeneratorType.MRG32K3A, 111))
            {
                var normalDistribution = new Normal(randomStream, 0, 1);

                for (int i = 0; i < vectors.Length; ++i)
                {
                    vectors[i] = NArray.CreateRandom(vectorLength, normalDistribution);
                }
            }
            var watch = new System.Diagnostics.Stopwatch(); watch.Start();
            var result = NArray.CreateLike(vectors.First());
            for (int i = 0; i < vectors.Length; ++i)
            {
                for (int j = 0; j < vectors.Length; ++j)
                {
                    result.Add(vectors[i] * vectors[j] * 123);
                    //result = result + vectors[i] * vectors[j] * 123;
                }
            }
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);

            var arrays = vectors.Select(v => (v.Storage as ManagedStorage<double>).Array).ToArray();

            watch.Restart();
            var arrayResult = new double[vectors.First().Length];
            for (int i = 0; i < vectors.Length; ++i)
            {
                for (int j = 0; j < vectors.Length; ++j)
                {
                    for (int k = 0; k < arrayResult.Length; ++k)
                    {
                        arrayResult[k] += arrays[i][k] * arrays[j][k] * 123;
                    }
                }
            }
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);

            watch.Restart();
            result = NArray.CreateLike(vectors.First());
            for (int i = 0; i < vectors.Length; ++i)
            {
                var batch = NArray.Evaluate(() =>
                {
                    NArray res = NArray.CreateScalar(0);
                    for (int j = 0; j < vectors.Length; ++j)
                    {
                        res = res + vectors[i] * vectors[j] * 123;
                    }
                    return res;
                });
                result += batch;
            }
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
        }
    }
}
