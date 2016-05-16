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
    public class BasicDifferentiationTests
    {
        public void TestDivision()
        {
            var stream = new RandomNumberStream(StorageLocation.Host);
            var test = NArray.CreateRandom(1000, new Normal(stream, 0, 1));
            var test2 = NArray.CreateRandom(1000, new Normal(stream, 0, 1));

            // first test

            var obtained1 = NArray.Evaluate(() =>
            {
                return 5.0 / test;
            }, test);

            var expected1 = 5.0 / test;
            var expectedDiff1 = -5.0 / (test * test);

            var pass = TestHelpers.Checkit(obtained1[0], expected1);
            pass = pass && TestHelpers.Checkit(obtained1[1], expectedDiff1);

            // second test

            var obtained2 = NArray.Evaluate(() =>
            {
                return test / test2;
            }, test, test2);

            var expected2 = test / test2;
            var expectedDiff2_1 = 1 / test2;
            var expectedDiff2_2 = -test / (test2 * test2);

            pass = TestHelpers.Checkit(obtained2[0], expected2);
            pass = pass && TestHelpers.Checkit(obtained2[1], expectedDiff2_1);
            pass = pass && TestHelpers.Checkit(obtained2[2], expectedDiff2_2);

        }
    }
}
