using System;
using System.Linq;
using System.Collections.Generic;
using VectorAccelerator;
using VectorAccelerator.NArrayStorage;
using VectorAccelerator.Distributions;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace VectorAccelerator.Tests
{
    public class ArrayPosition
    {
        public readonly double[] Array1;
        public readonly double[] Array2;

        public ArrayPosition(double[] array1, double[] array2)
        {
            Array1 = array1;
            Array2 = array2;
        }
    }

    public class VectorPosition
    {
        public readonly NArray Array1;
        public readonly NArray Array2;

        public VectorPosition(NArray array1, NArray array2)
        {
            Array1 = array1;
            Array2 = array2;
        }
    }

    [TestFixture]
    public class MultiplyAggregateTests
    {
        /// <summary>
        /// A test of raw performance for performing a simple aggregation task (no transandentals, not compute-bound).
        /// </summary>
        public void TestPerformance()
        {
            var length = 5000;
            var count = 200; 
            double[][] arrays;
            NArray[] vectors;
            GetArrays(count, length, out vectors, out arrays);
            var arrayPositions = new List<ArrayPosition>();
            var vectorPositions = new List<VectorPosition>();

            for (int i = 0; i < count; ++i)
            {
                for (int j = 0; j < count; ++j)
                {
                    arrayPositions.Add(new ArrayPosition(arrays[i], arrays[j]));
                    vectorPositions.Add(new VectorPosition(vectors[i], vectors[j]));
                }
            }

            int vectorLength = vectors.First().Length;

            var watch = new System.Diagnostics.Stopwatch(); watch.Start();
            var arraySum = RunArray(arrayPositions);
            watch.Stop();
            Console.WriteLine(string.Format("Array parallel: {0:F3} ms", watch.ElapsedMilliseconds));

            watch.Restart();
            var vectorSum = RunVector(vectorPositions);
            watch.Stop();
            Console.WriteLine(string.Format("Vector parallel: {0:F3} ms", watch.ElapsedMilliseconds));

            watch.Restart();
            var vectorSumDeferred = RunVectorDeferred(vectorPositions);
            watch.Stop();
            Console.WriteLine(string.Format("Deferred vector parallel: {0:F3} ms", watch.ElapsedMilliseconds));

            Assert.IsTrue(TestHelpers.AgreesAbsolute(vectorSumDeferred.DebugDataView.ToArray(), arraySum));
        }

        public static double[] RunArray(List<ArrayPosition> arrayPositions)
        {
            var length = arrayPositions.First().Array1.Length;
            var sum = new double[length];

            var options = new ParallelOptions() { MaxDegreeOfParallelism = 1 };
            var rangePartitioner = Partitioner.Create(0, arrayPositions.Count);

            object lockObject = new object();

            Parallel.ForEach(
              rangePartitioner,
              options,
              () => new double[length],
              (range, loopState, initialValue) =>
              {
                  var partialSum = initialValue;
                  for (int i = range.Item1; i < range.Item2; i++)
                  {
                      var position = arrayPositions[i];
                      NArray res = NArray.CreateScalar(0);
                      for (int k = 0; k < length; ++k)
                      {
                          partialSum[k] += position.Array1[k] * position.Array2[k] * 10;
                      }

                  }
                  return partialSum;
              },
              (localPartialSum) =>
              {
                  lock (lockObject)
                  {
                      for (int k = 0; k < length; ++k) sum[k] += localPartialSum[k];
                  }
              });

            return sum;
        }

        public static NArray RunVector(List<VectorPosition> vectorPositions)
        {
            var length = vectorPositions.First().Array1.Length;
            var sum = new NArray(StorageLocation.Host, length);

            var options = new ParallelOptions() { MaxDegreeOfParallelism = 1 };
            var rangePartitioner = Partitioner.Create(0, vectorPositions.Count);

            object lockObject = new object();

            Parallel.ForEach(
              rangePartitioner,
              options,
              () => new NArray(StorageLocation.Host, length),
              (range, loopState, initialValue) =>
              {
                  var partialSum = initialValue;
                  for (int i = range.Item1; i < range.Item2; i++)
                  {
                      var position = vectorPositions[i];
                      partialSum += position.Array1 * position.Array2 * 10;
                  }
                  return partialSum;
              },
              (localPartialSum) =>
              {
                  lock (lockObject)
                  {
                      sum += localPartialSum;
                  }
              });

            return sum;
        }

        public static NArray RunVectorDeferred(List<VectorPosition> vectorPositions)
        {
            var length = vectorPositions.First().Array1.Length;
            var sum = new NArray(StorageLocation.Host, length);

            var options = new ParallelOptions() { MaxDegreeOfParallelism = 1 };
            var rangePartitioner = Partitioner.Create(0, vectorPositions.Count);

            object lockObject = new object();

            Parallel.ForEach(
              rangePartitioner,
              options,
              () => new NArray(StorageLocation.Host, length),
              (range, loopState, initialValue) =>
              {
                  var batchSum = NArray.Evaluate(() =>
                  {
                      var partialSum = initialValue;
                      for (int i = range.Item1; i < range.Item2; i++)
                      {
                          var position = vectorPositions[i];
                          partialSum += position.Array1 * position.Array2 * 10;
                      }
                      return partialSum;
                  });
                  return batchSum;
              },
              (localPartialSum) =>
              {
                  lock (lockObject)
                  {
                      sum += localPartialSum;
                  }
              });

            return sum;
        }

        public NArray CalculateDeferred(NArray[] vectors)
        {
            var result = NArray.CreateLike(vectors.First());
            for (int i = 0; i < vectors.Length; ++i)
            {
                var batch = NArray.Evaluate(() =>
                {
                    NArray res = NArray.CreateScalar(0);
                    for (int j = 0; j < vectors.Length; ++j)
                    {
                        res = res + vectors[i] * vectors[j] * 10;
                    }
                    return res;
                });
                result += batch;
            }
            return result;
        }

        public void GetArrays(int count, int length, out NArray[] vectors, out double[][] arrays)
        {
            vectors = new NArray[count]; 
            using (var randomStream = new RandomNumberStream(StorageLocation.Host, RandomNumberGeneratorType.MRG32K3A, 111))
            {
                var normalDistribution = new Normal(randomStream, 0, 1);

                for (int i = 0; i < vectors.Length; ++i)
                {
                    vectors[i] = NArray.CreateRandom(length, normalDistribution);
                }
            }
            arrays = vectors.Select(v => (v.Storage as ManagedStorage<double>).Data).ToArray();
        }
    }
}
