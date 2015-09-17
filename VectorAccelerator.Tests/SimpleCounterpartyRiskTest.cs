using System;
using System.Linq;
using VectorAccelerator;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VectorAccelerator.NArrayStorage;
using System.Threading.Tasks;
using VectorAccelerator.LinearAlgebraProviders;
using VectorAccelerator.DeferredExecution;

namespace VectorAccelerator.Tests
{
    [TestClass]
    public class SimpleCounterpartyRiskTest
    {
        [TestMethod]
        public void OptionPricingTest()
        {
            IntelMathKernalLibrary.SetAccuracyMode(VMLAccuracy.LowAccuracy);
            IntelMathKernalLibrary.SetSequential();

            using (var random = new IntelMKLRandomNumberGenerator(RandomNumberGeneratorType.MRG32K3A, 111))
            {
                
                var vectorOptions = new VectorExecutionOptions() { MultipleThreads = true };

                var watch = new Stopwatch(); watch.Start();

                for (int i = 0; i < 1; ++i)
                {
                    var optionPrices = new NArray(5000);
                    CalculatePVs(random, optionPrices);
                }
                Console.WriteLine(watch.ElapsedMilliseconds); watch.Restart();

                Parallel.For(0, 1, (i) =>
                {
                    var optionPrices = new NArray(5000);
                    CalculatePVs(random, optionPrices);
                });
                Console.WriteLine(watch.ElapsedMilliseconds); watch.Restart();

                for (int i = 0; i < 1; ++i)
                {
                    var optionPrices2 = new NArray(5000);
                    using (NArray.DeferredExecution(vectorOptions))
                    {
                        CalculatePVs(random, optionPrices2);
                    }
                }

                Console.WriteLine(watch.ElapsedMilliseconds); watch.Restart();
                
                
                //Console.WriteLine(TestHelpers.CheckitString(optionPrices, optionPrices2));
            }
        }

        private void CalculatePVs(IRandomNumberGenerator random, NArray optionPrices)
        {
            double deltaT = 1;
            double r = 0.1;
            double vol = 0.3;

            var variates = NArray.CreateRandom(optionPrices.Length, random);

            var logStockPrice = Math.Log(100)
                + variates * Math.Sqrt(deltaT) * vol + (r - 0.5 * vol * vol) * deltaT;
            
            var forwardStockPrices = NMath.Exp(logStockPrice + r * deltaT);

            for (double k = 80; k < 81; ++k)
            {
                optionPrices += Finance.BlackScholes(-1, forwardStockPrices, 90, 0.2, 1);
            }
        }
    }
}
