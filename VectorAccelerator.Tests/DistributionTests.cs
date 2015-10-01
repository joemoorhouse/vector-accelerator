using System;
using System.Linq;
using VectorAccelerator;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VectorAccelerator.NArrayStorage;
using System.Threading.Tasks;
using VectorAccelerator;
using VectorAccelerator.Distributions;
using VectorAccelerator.LinearAlgebraProviders;

namespace VectorAccelerator.Tests
{
    public class DistributionTests
    {
        [TestMethod]
        public void NonNormalVariateGeneration()
        {

        }
        
        [TestMethod]
        public void TestRandomNumberGeneration()
        {            
            // We create the stream.
            // This identifies the stream and stores the state or pointer to the state (in the case
            // of IntelMKL, CUDA, etc).
            using (var randomStream = new RandomNumberStream(RandomNumberGeneratorType.MRG32K3A, 111))
            {
                // We then create a distribution based on the stream.
                // This stores only specific parameters of the distribution (i.e. mean and standard deviation).
                // The purpose of the design is to push scaling and offsetting of random numbers to the Provider,
                // which can do this efficiently whilst generating.
                var normal = new Normal(randomStream, 0, 1);

                var a = new NArray(1000);
                var b = new NArray(1000);

                // When we call FillRandom, we need to use a Provider that is appropriate to the stream.
                a.FillRandom(normal);
                b.FillRandom(normal);

                var v = a * b;
            }
        }
    }
}
