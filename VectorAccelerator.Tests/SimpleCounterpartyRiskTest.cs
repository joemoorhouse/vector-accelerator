using System;
using System.Linq;
using VectorAccelerator;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VectorAccelerator.NArrayStorage;
using System.Threading.Tasks;
using VectorAccelerator.Distributions;
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
            var a = new NArray(Enumerable.Range(0, 10).Select(i => (double)i).ToArray());
            var b = new NArray(Enumerable.Range(0, 10).Select(i => (double)i * 2).ToArray());
            var c = 5 - b;
            var check = c.First();
            IntelMathKernelLibrary.SetAccuracyMode(VMLAccuracy.LowAccuracy);
            IntelMathKernelLibrary.SetSequential();
            using (var randomStream = new RandomNumberStream(RandomNumberGeneratorType.MRG32K3A, 111))
            {
                var normalDistribution = new Normal(randomStream, 0, 1);

                var vectorOptions = new VectorExecutionOptions() { MultipleThreads = true };

                var watch = new Stopwatch();

                watch.Start();
                var optionPrices = Value(normalDistribution);
                Console.WriteLine(String.Format("Start-up: {0}ms", watch.ElapsedMilliseconds)); 

                randomStream.Reset();

                watch.Restart();
                optionPrices = Value(normalDistribution);
                Console.WriteLine(String.Format("Threaded, deferred 1: {0}ms", watch.ElapsedMilliseconds)); 

                randomStream.Reset();

                watch.Restart();
                var optionPricesDeferred = Value(normalDistribution);
                Console.WriteLine(String.Format("Threaded, deferred 2: {0}ms", watch.ElapsedMilliseconds)); watch.Restart();
                
                Console.WriteLine(TestHelpers.CheckitString(optionPrices, optionPricesDeferred));
            }
        }

        private NArray Value(Normal normalDistribution)
        {
            double deltaT = 1;
            double r = 0.1;
            double vol = 0.3;

            int vectorLength = 5000;
            var optionPrices = new NArray(vectorLength);

            var variates = NArray.CreateRandom(optionPrices.Length, normalDistribution);

            var logStockPrice = Math.Log(100)
                + variates * Math.Sqrt(deltaT) * vol + (r - 0.5 * vol * vol) * deltaT;
            
            var forwardStockPrices = NMath.Exp(logStockPrice + r * deltaT);

            // now create deals
            var strikes = Enumerable.Range(0, 1000).Select(i => 80 + 5.0 / 1000).ToArray();
            var deals = strikes.Select(s =>
                new Deal() { Strike = s, ForwardStockPrices = (i) => { return forwardStockPrices; } })
                .ToList();

            return AggregateValuations(deals, vectorLength);
        }

        public delegate NArray Spot(int index);
        public delegate NArray Curve(int index, double tenor);

        public class Deal
        {
            public Spot ForwardStockPrices;

            public double Strike;

            public NArray Price(int timeIndex)
            {
                return Finance.BlackScholes(-1, ForwardStockPrices(timeIndex), Strike, 0.2, 1);
            }
        }
        
        /// <summary>
        /// A very customisable aggregation routine
        /// </summary>
        /// <param name="deals"></param>
        /// <param name="vectorLength"></param>
        /// <returns></returns>
        private NArray AggregateValuations(IList<Deal> deals, int vectorLength)
        {
            object lockObject = new object();
            var rangePartitioner = Partitioner.Create(0, deals.Count);

            var sum = new NArray(vectorLength);

            var options = new ParallelOptions();// { MaxDegreeOfParallelism = 2 };

            Parallel.ForEach(
                // input intervals
              rangePartitioner,

              options,

              // local initial partial result
              () => new NArray(vectorLength),

              // loop body for each interval
              (range, loopState, initialValue) =>
              {
                  var partialSum = initialValue;
                  for (int i = range.Item1; i < range.Item2; i++)
                  {
                      using (NArray.DeferredExecution())
                      {
                          partialSum.Add(deals[i].Price(0));
                      }
                  }
                  return partialSum;
              },

              // final step of each local context
              (localPartialSum) =>
              {
                  // using a lock to enforce serial access to shared result
                  lock (lockObject)
                  {
                      sum.Add(localPartialSum);
                  }
              });
            
            return sum;
        }    

        public enum PutCall { Put, Call }
    }
}
