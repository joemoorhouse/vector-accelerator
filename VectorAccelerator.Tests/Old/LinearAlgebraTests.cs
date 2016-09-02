using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator;
using VectorAccelerator.Distributions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VectorAccelerator.Tests
{
    public class LinearAlgebraTests
    {
        public void TestCorrelation()
        {
            var factory = new NArrayFactory(StorageLocation.Host);

            var a = factory.CreateNArray(new double[] { 0.32, 0.91, -0.32, -0.25 });
            var b = factory.CreateNArray(new double[] { 0.49, 0.75, -0.39, -0.20 });

            var correlation = NMath.Correlation(a, b);
  
            Assert.AreEqual(Math.Round(correlation, 12), Math.Round(0.96831734966276, 12));

        }
    }
}
