using System;
using System.Linq;
using System.Collections.Generic;
using VectorAccelerator;
using VectorAccelerator.NArrayStorage;
using VectorAccelerator.Distributions;
using VectorAccelerator.LinearAlgebraProviders;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace VectorAccelerator.Tests
{
    public class NaNTest
    {
        public void TestNaNPerformance()
        {
            int arrayCount = 1000;
            NArray[] withoutNaNs = new NArray[arrayCount];
            NArray[] withNaNs = new NArray[arrayCount];
            using (var randomStream = new RandomNumberStream(StorageLocation.Host, RandomNumberGeneratorType.MRG32K3A, 111))
            {
                var normalDistribution = new Normal(randomStream, 0, 1);

                for (int i = 0; i < arrayCount; ++i)
                {
                    //withoutNaNs[i] = NArray.CreateRandom(5000, normalDistribution) + 10;
                    withNaNs[i] = NArray.CreateRandom(5000, normalDistribution);
                    var array = (withNaNs[i].Storage as ManagedStorage<double>).Data;
                    withoutNaNs[i] = new NArray(StorageLocation.Host, array.Select(v => Math.Max(0.0, v)).ToArray());
                }
            }

            TestHelpers.Timeit(() =>
            {
                for (int i = 0; i < arrayCount; ++i)
                {
                    var array = (withoutNaNs[i].Storage as ManagedStorage<double>).Data;
                    IntelMathKernelLibrary.InverseSquareRoot(array, 0, array, 0, 5000);
                }
            }, 10, 1);

            TestHelpers.Timeit(() =>
            {
                for (int i = 0; i < arrayCount; ++i)
                {
                    var array = (withNaNs[i].Storage as ManagedStorage<double>).Data;
                    IntelMathKernelLibrary.InverseSquareRoot(array, 0, array, 0, 5000);
                }
            }, 10, 1);

        }
    }
}
