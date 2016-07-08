using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VectorAccelerator.Distributions;
using VectorAccelerator.LinearAlgebraProviders;
using VectorAccelerator.NArrayStorage;

namespace VectorAccelerator.Tests
{
    [TestClass]
    public class SpeedComparison
    {
        public void OptionPricingTest()
        {
            var location = StorageLocation.Host;
            
            IntelMathKernelLibrary.SetAccuracyMode(VMLAccuracy.HighAccuracy);

            using (var randomStream = new RandomNumberStream(location, RandomNumberGeneratorType.MRG32K3A, 111))
            {
                var normalDistribution = new Normal(randomStream, 0, 1);

                var optionPricesVector = new NArray(location, 5000);
                var optionPricesIndexed = new NArray(location, 5000);

                var variates = NArray.CreateRandom(optionPricesVector.Length, normalDistribution);

                TestHelpers.Timeit(() =>
                {
                    VectorForm(variates, optionPricesVector);
                });

                //TestHelpers.Timeit(() =>
                //{
                //    using (NArray.DeferredExecution()) 
                //    {
                //        VectorForm(variates, optionPricesVector);
                //    }                
                //});

                TestHelpers.Timeit(() =>
                {
                    IndexedForm((variates.Storage as ManagedStorage<double>).Array, 
                        (optionPricesIndexed.Storage as ManagedStorage<double>).Array);
                });
                Console.WriteLine(
                    TestHelpers.AgreesAbsoluteString(optionPricesVector, optionPricesIndexed));
            }
        }
        
        public static void VectorForm(NArray variates, NArray optionPrices)
        {
            double deltaT = 1;
            double r = 0.1;
            double vol = 0.3;
            
            var logStockPrice = Math.Log(100)
                + variates * Math.Sqrt(deltaT) * vol + (r - 0.5 * vol * vol) * deltaT;

            var forwardStockPrices = NMath.Exp(logStockPrice + r * deltaT);

            optionPrices.Assign(Finance.BlackScholes(-1, forwardStockPrices, 90, 0.2, 1));
        }

        public static void IndexedForm(double[] variates, double[] optionPrices)
        {
            double deltaT = 1;
            double r = 0.1;
            double vol = 0.3;

            for (int i = 0; i < variates.Length; ++i)
            {
                double logStockPrice = Math.Log(100)
                + variates[i] * Math.Sqrt(deltaT) * vol + (r - 0.5 * vol * vol) * deltaT;

                var forwardStockPrices = Math.Exp(logStockPrice + r * deltaT);

                optionPrices[i] = BlackScholes(-1, forwardStockPrices, 90, 0.2, 1);
            }    
        }

        public static double BlackScholes(double putCallFactor, double forward, double strike,
            double volatility, double deltaTime)
        {
            double d1, d2;
            if (deltaTime == 0) return putCallFactor * (forward - strike);

            BlackScholesD1D2Parameters(forward, strike, volatility, deltaTime, out d1, out d2);

            var nd1 = Normal.CumulativePDF(putCallFactor * d1);
            var nd2 = Normal.CumulativePDF(putCallFactor * d2);

            return putCallFactor * (forward * nd1 - strike * nd2);
        }

        public static void BlackScholesD1D2Parameters(double forward, double strike, double volatility,
            double deltaTime, out double d1, out double d2)
        {
            var volMultSqrtDTime = volatility * Math.Sqrt(deltaTime);

            d1 = (Math.Log(forward / strike) + (0.5 * volatility * volatility * deltaTime))
                / volMultSqrtDTime;

            d2 = d1 - volMultSqrtDTime;
        }
    }
}
