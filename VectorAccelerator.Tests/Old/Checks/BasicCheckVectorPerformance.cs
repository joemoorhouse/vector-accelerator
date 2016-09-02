using System;
using System.Linq;
using VectorAccelerator;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VectorAccelerator.NArrayStorage;
using System.Threading.Tasks;
using VectorAccelerator.Distributions;
using VectorAccelerator.LinearAlgebraProviders;

namespace VectorAccelerator.Tests
{
    public class BasicCheckVectorPerformance
    {
        public void CheckExponentialPerformance()
        {
            IntelMathKernelLibrary.SetSequential();

            var location = StorageLocation.Host;

            using (var randomStream = new RandomNumberStream(location, RandomNumberGeneratorType.MRG32K3A, 111))
            {
                var normalDistribution = new Normal(randomStream, 0, 1);

                var a = new NArray(location, 5000);
                var b = new NArray(location, 5000);
                a.FillRandom(normalDistribution);

                var aArray = GetArray(a);
                var bArray = GetArray(b);

                var watch = new Stopwatch(); watch.Start();

                for (int i = 0; i < 1000; ++i)
                {
                    IntelMathKernelLibrary.Exp(aArray, 0, bArray, 0, 5000);
                }

                var baseline = watch.ElapsedMilliseconds;
                Console.WriteLine("5 millions in place Exps");
                Console.WriteLine(baseline);
            }
        }

        private double[] GetArray(NArray a)
        {
            return (a.Storage as ManagedStorage<double>).Array;
        }
    }
}
