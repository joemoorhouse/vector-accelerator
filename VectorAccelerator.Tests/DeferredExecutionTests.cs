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
    class DeferredExecutionTests
    {
        public void SimpleDeferredOperationsTest()
        {
            var location = StorageLocation.Host;
            var factory = new NArrayFactory(location);

            using (var randomStream = 
                new RandomNumberStream(location, RandomNumberGeneratorType.MRG32K3A, 111))
            {
                var normalDistribution = new Normal(randomStream, 0, 1);

                var epsilon = factory.CreateNArray(1000, 1);
                epsilon.FillRandom(normalDistribution);

                var x = epsilon * Math.Sqrt(0.25) * 0.2;
            }
        }
    }
}
