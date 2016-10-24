
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VectorAccelerator.Distributions;

namespace VectorAccelerator.Tests
{
    [TestClass]
    public class BasicDifferentiationTests
    {
        [TestMethod]
        public void DivisionTest()
        {
            NArray a, b;
            using (var stream = new RandomNumberStream(StorageLocation.Host))
            {
                a = NArray.CreateRandom(1000, new Normal(stream, 0, 1));
                b = NArray.CreateRandom(1000, new Normal(stream, 0, 1));
            }

            // First test
            var expected1 = 5.0 / a;
            var expectedDiff1 = -5.0 / (a * a);

            var obtained1 = NArray.Evaluate(() =>
            {
                return 5.0 / a;
            }, a);


            Assert.IsTrue(TestHelpers.AgreesAbsolute(obtained1[0], expected1));
            Assert.IsTrue(TestHelpers.AgreesAbsolute(obtained1[1], expectedDiff1));

            // Second test
            var expected2 = a / b;
            var expectedDiff2_1 = 1 / b;
            var expectedDiff2_2 = -a / (b * b);

            var obtained2 = NArray.Evaluate(() =>
            {
                return a / b;
            }, a, b);

            Assert.IsTrue(TestHelpers.AgreesAbsolute(obtained2[0], expected2));
            Assert.IsTrue(TestHelpers.AgreesAbsolute(obtained2[1], expectedDiff2_1));
            Assert.IsTrue(TestHelpers.AgreesAbsolute(obtained2[2], expectedDiff2_2));
        }
    }
}
