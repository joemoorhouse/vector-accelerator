using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VectorAccelerator;
using RiskEngine.Framework;
using RiskEngine.Pricers;
using RiskEngine.Calibration;
using RiskEngine.Factors;
using RiskEngine.Models;
using RiskEngine.Data;

namespace VectorAccelerator.Tests
{
    [TestClass]
    public class BasicSwapAADTest
    {
        /// <summary>
        /// Calculate exposure profile for uncollateralised interest rate swap trade
        /// and derivatives against all points on time zero rates curve.
        /// </summary>
        [TestMethod]
        public void TestEndToEnd()
        {
            LinearGaussianModel model;
            IEnumerable<NArray> allZeroRatesT0;
            TimePoint[] timePoints;
            SimulationGraph graph;
            List<IPricer> pricers;

            SimulateModel(out model, out allZeroRatesT0, out timePoints, out graph, out pricers);

            var simulationCount = graph.Context.Settings.SimulationCount;
            var timePointCount = graph.Context.Settings.SimulationTimePoints.Length;

            var resultStorage = 
                Enumerable.Range(0, timePointCount).Select(i =>
                Enumerable.Range(0, 1 + allZeroRatesT0.Count()).
                Select(j => new NArray(StorageLocation.Host, simulationCount, 1)).ToArray()).
                ToArray();

            var resultStorageSingle = Enumerable.Range(0, timePointCount).Select(i =>
                new List<NArray> { new NArray(StorageLocation.Host, 
                simulationCount, 1) })
                .ToArray();

            Console.WriteLine(string.Format("Using {0} scenarios", simulationCount));

            foreach (var pricer in pricers) pricer.PrePrice();

            Console.WriteLine(); Console.WriteLine("Deferred execution single flow, single thread");
            var tempStorage = new List<NArray> { new NArray(StorageLocation.Host, 
                simulationCount, 1) };

            Console.WriteLine(); Console.WriteLine(string.Format("Deferred execution, {0} flows, no derivatives, single thread", pricers.Count));
            double averageTime;
            TestHelpers.Timeit(() =>
            {
                for (int i = 0; i < timePointCount; ++i)
                {
                    foreach (var pricer in pricers)
                    {
                        NArray.Evaluate(() =>
                        {
                            NArray pv;
                            pricer.Price(i, out pv);
                            return pv;
                        },
                        new List<NArray>(), Aggregator.ElementwiseAdd, resultStorageSingle[i]);
                    }
                }
            }, out averageTime, 1, 1);

            var pricingOperationCount = 0;
            for (int i = 0; i < timePointCount; ++i)
            {
                pricingOperationCount += pricers.Count(p => p.ExposureEndDate < timePoints[i].DateTime);
            }

            double timeForSinglePrice = averageTime * 1e6 / (pricingOperationCount * simulationCount);
            Console.WriteLine(string.Format("Average time for single flow: {0:F1} ns", timeForSinglePrice));

            var percentiles = new double[] { 1, 10, 50, 90, 99 };
            var measures = new List<IList<double>>();
            for (int i = 0; i < timePoints.Length; ++i)
            {
                var values = NMath.Percentiles(resultStorageSingle[i].First(), percentiles)
                    .Concat(new double[] { resultStorageSingle[i].First().DebugDataView
                        .Select(v => Math.Max(0, v)).Average() })
                    .ToList();
                measures.Add(values);
            }

            var times = timePoints.Select(p => p.YearsFromBaseDate).ToArray();
            var profile10 = measures.Select(p => p[1]).ToArray();
            var profile90 = measures.Select(p => p[3]).ToArray();
            var meanPositive = measures.Select(p => p[5]).ToArray();
            var maxMeanPositive = meanPositive.Max();
            var maxMeanPositiveIndex = Array.IndexOf(meanPositive, maxMeanPositive);

            //Plot.PlotHelper.QuickPlot(times, profile90, new Tuple<double,double>(0, 11));

            Console.WriteLine(); Console.WriteLine(string.Format("Deferred execution, {0} flows, {1} derivatives, single thread", pricers.Count, allZeroRatesT0.Count()));
            TestHelpers.Timeit(() =>
            {
                for (int i = 0; i < timePointCount; ++i)
                {
                    foreach (var pricer in pricers)
                    {
                        NArray.Evaluate(() =>
                        {
                            NArray pv;
                            pricer.Price(i, out pv);
                            return pv;
                        },
                        allZeroRatesT0.ToArray(), Aggregator.ElementwiseAdd, resultStorage[i]);
                    }
                }
            }, 1, 1);

            // we check that 
            var maxMeanPositiveSims = resultStorage[maxMeanPositiveIndex].First();
            List<double> gradients = new List<double>();
            for (int i = 0; i < allZeroRatesT0.Count(); ++i)
            {
                var maxMeanPositiveDerivs = resultStorage[maxMeanPositiveIndex][i + 1];
                gradients.Add(maxMeanPositiveSims.DebugDataView
                    .Zip(maxMeanPositiveDerivs.DebugDataView,
                    (s, d) => (s > 0) ? d : 0)
                    .Average()
                    );
            }

            Assert.IsTrue(TestHelpers.AgreesAbsolute(gradients[16], 8748.38858299));

            Console.WriteLine(string.Format(
                "Calculated gradient of maximum positive exposure ({0:F2} EUR) to change of the zero rate with maturity {1:F2} years: {2:F3}",
                maxMeanPositive, 2556 / 365.25, gradients[16]));
        }
        
        /// <summary>
        /// Test building blocks of interest rate swap exposure calculation.
        /// </summary>
        [TestMethod]
        public void TestBasics()
        {
            LinearGaussianModel model;
            IEnumerable<NArray> allZeroRatesT0;
            TimePoint[] timePoints;
            SimulationGraph graph;
            List<IPricer> pricers;

            SimulateModel(out model, out allZeroRatesT0, out timePoints, out graph, out pricers);
            TestBasics(model, allZeroRatesT0, timePoints, graph);
        }

        /// <summary>
        /// Test building blocks of interest rate swap exposure calculation.
        /// </summary>
        public void TestBasics(LinearGaussianModel model, IEnumerable<NArray> allZeroRatesT0,
            TimePoint[] timePoints, SimulationGraph graph)
        {
            bool testModel = true;
            if (testModel)
            {
                var check1 = model[1, timePoints[1].DateTime.AddDays(182)].First();
                Assert.IsTrue(TestHelpers.AgreesAbsolute(check1, 1.0015314301020275));

                var check2 = model[1, timePoints[1].DateTime.AddDays(91)].First();
                Assert.IsTrue(TestHelpers.AgreesAbsolute(check2, 1.0007451895710209));

                var check3 = model.ForwardRate(1,
                    timePoints[1].DateTime.AddDays(91), timePoints[1].DateTime.AddDays(182)).First();
                Assert.IsTrue(TestHelpers.AgreesAbsolute(check3, -0.0031509366920208916));
            }

            bool testProfile = true;
            if (testProfile)
            {
                var testVariates0 = graph.RegisterFactor<NormalVariates>("IR_DiscountFactor_EUR_Factor0");
                var testVariates1 = graph.RegisterFactor<NormalVariates>("IR_DiscountFactor_EUR_Factor1");
                var testVariates2 = graph.RegisterFactor<NormalVariates>("IR_DiscountFactor_EUR_Factor2");
                
                var check = Descriptive.Correlation(testVariates1.Value, testVariates2.Value);

                // check 3M rolling tenor
                var percentiles = new double[] { 1, 10, 50, 90, 99 };
                var measures = new List<IList<double>>();
                for (int i = 0; i < timePoints.Length; ++i)
                {
                    var tenorMonths = 3;
                    var tenorYears = tenorMonths / 12.0;
                    var df = model[i, timePoints[i].DateTime.AddMonths(3)];
                    var zeroRate = -NMath.Log(df) / tenorYears;
                    var values = NMath.Percentiles(zeroRate, percentiles).ToList();
                    measures.Add(values);
                }

                var times = timePoints.Select(p => p.YearsFromBaseDate).ToArray();
                var profile10 = measures.Select(p => p[1]).ToArray();
                var profile90 = measures.Select(p => p[3]).ToArray();
            }

            bool testForwardRate = true;
            if (testForwardRate)
            {
                // check 3M rolling tenor
                var percentiles = new double[] { 1, 10, 50, 90, 99 };
                var measures = new List<IList<double>>();
                for (int i = 0; i < timePoints.Length; ++i)
                {
                    var tenorMonths = 3;
                    var tenorYears = tenorMonths / 12.0;
                    var forwardRate = model.ForwardRate(i, timePoints[i].DateTime.AddMonths(3), timePoints[i].DateTime.AddMonths(6));
                    var values = NMath.Percentiles(forwardRate, percentiles).ToList();
                    measures.Add(values);
                }

                var times = timePoints.Select(p => p.YearsFromBaseDate).ToArray();
                var profile10 = measures.Select(p => p[1]).ToArray();
                var profile90 = measures.Select(p => p[3]).ToArray();
            }

            bool testAAD = true;
            if (testAAD)
            {
                // just get the result
                var result0 = NArray.Evaluate(() =>
                {
                    var df = model[10, timePoints[10].DateTime.AddMonths(3)];
                    return df;
                });
                
                // get the result and single derivative
                var result1 = NArray.Evaluate(() =>
                {
                    var df = model[10, timePoints[10].DateTime.AddMonths(3)];
                    return df;
                }, model.ZeroRatesT0[6].Value);

                var log = new StringBuilder();

                // get the result and all derivatives (and log output)
                var result2 = NArray.Evaluate(() =>
                {
                    var df = model[10, timePoints[10].DateTime.AddMonths(3)];
                    return df;
                }, log, allZeroRatesT0.ToArray());

                // now forward rate and all derivatives
                var result3 = NArray.Evaluate(() =>
                {
                    var df = model.ForwardRate(10, timePoints[10].DateTime.AddMonths(3), timePoints[10].DateTime.AddMonths(6));
                    return df;
                }, allZeroRatesT0.ToArray());

                var unbumped0 = model[10, timePoints[10].DateTime.AddMonths(3)];

                var expected0 = unbumped0.DebugDataView.ToArray();
                var unbumped3 = model.ForwardRate(10, timePoints[10].DateTime.AddMonths(3), timePoints[10].DateTime.AddMonths(6));
                var expected3 = unbumped3.DebugDataView.ToArray();

                var obtained0 = result1[0].DebugDataView.ToArray();
                var obtained1 = result1[0].DebugDataView.ToArray();
                var obtained2 = result2[0].DebugDataView.ToArray();
                var obtained3 = result3[0].DebugDataView.ToArray();

                Assert.IsTrue(TestHelpers.AgreesAbsolute(expected0, obtained0));
                Assert.IsTrue(TestHelpers.AgreesAbsolute(expected0, obtained1));
                Assert.IsTrue(TestHelpers.AgreesAbsolute(expected0, obtained2));
                Assert.IsTrue(TestHelpers.AgreesAbsolute(expected3, obtained3));

                var logCheck = log.ToString();

                var obtained_deriv1 = result1[1].DebugDataView.ToArray();
                var obtained_deriv2 = result2[7].DebugDataView.ToArray();
                var obtained_deriv3 = result3[7].DebugDataView.ToArray();

                model.ZeroRatesT0.Data[6] = new DataPoint(model.ZeroRatesT0.Data[6].Time, model.ZeroRatesT0.Data[6].Value + 1e-6);

                var bumped0 = model[10, timePoints[10].DateTime.AddMonths(3)];
                var bumped3 = model.ForwardRate(10, timePoints[10].DateTime.AddMonths(3), timePoints[10].DateTime.AddMonths(6));

                var expected_deriv1 = ((bumped0 - unbumped0) / 1e-6)
                    .DebugDataView.ToArray();

                var expected_deriv3 = ((bumped3 - unbumped3) / 1e-6)
                    .DebugDataView.ToArray();

                Assert.IsTrue(TestHelpers.AgreesAbsolute(expected_deriv1, obtained_deriv1, 1e-5));
                Assert.IsTrue(TestHelpers.AgreesAbsolute(expected_deriv1, obtained_deriv2, 1e-5));
                Assert.IsTrue(TestHelpers.AgreesAbsolute(expected_deriv3, obtained_deriv3, 1e-5));  
            }

            bool checkTimings = true;
            if (checkTimings)
            {
                // check timings
                Console.WriteLine("Immediate execution");
                VectorAccelerator.Tests.TestHelpers.Timeit(() =>
                {
                    var df = model[10, timePoints[10].DateTime.AddMonths(3)];
                }, 10, 10);

                Console.WriteLine(); Console.WriteLine("Deferred execution no derivatives");
                VectorAccelerator.Tests.TestHelpers.Timeit(() =>
                {
                    NArray.Evaluate(() =>
                    {
                        var df = model[10, timePoints[10].DateTime.AddMonths(3)];
                        return df;
                    });
                }, 10, 10);

                Console.WriteLine(); Console.WriteLine("Deferred execution all derivatives");
                VectorAccelerator.Tests.TestHelpers.Timeit(() =>
                    {
                        NArray.Evaluate(() =>
                        {
                            var df = model[10, timePoints[10].DateTime.AddMonths(3)];
                            return df;
                        }, allZeroRatesT0.ToArray());
                    }, 10, 10);
            }
        }

        public void TestPerformance()
        {
            LinearGaussianModel model;
            IEnumerable<NArray> allDiscountFactorT0;
            TimePoint[] timePoints;
            SimulationGraph graph;
            List<IPricer> pricers;

            SimulateModel(out model, out allDiscountFactorT0, out timePoints, out graph, out pricers);

            var resultStorage = Enumerable.Range(0, 1 + allDiscountFactorT0.Count()).
                Select(i => new NArray(StorageLocation.Host, 5000, 1)).ToArray();

            Console.WriteLine(); Console.WriteLine("Deferred execution all derivatives, storage provided");
            VectorAccelerator.Tests.TestHelpers.Timeit(() =>
            {
                NArray.Evaluate(() =>
                {
                    var df = model[10, timePoints[10].DateTime.AddMonths(3)];
                    return df;
                },
                allDiscountFactorT0.ToArray(), Aggregator.ElementwiseAdd, resultStorage);
            }, 10, 10);
        }

        /// <summary>
        /// Simulate state variables needed for the pricing step of the exposure calculation. State variables
        /// are not independent variables of the adjoint algorithmic differentation, and therefore can be simulated in advance.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="allZeroRatesT0"></param>
        /// <param name="timePoints"></param>
        /// <param name="graph"></param>
        /// <param name="pricers"></param>
        public void SimulateModel(out LinearGaussianModel model, out IEnumerable<NArray> allZeroRatesT0,
            out TimePoint[] timePoints, out SimulationGraph graph, out List<IPricer> pricers)
        {
            var testDate = new DateTime(2015, 12, 1);

            graph = new SimulationGraph(StorageLocation.Host, testDate);
            var context = graph.Context;
            context.Settings.SimulationCount = 5000;

            var fixedLeg = Enumerable.Range(0, 40).Select(i => new FixedCashflowDeal()
            {
                Notional = -1e6,
                Rate = 0.002,
                Currency = Currency.EUR,
                StartDate = testDate.AddMonths(3 * i),
                EndDate = testDate.AddMonths(3 * (i + 1))
            });

            var floatingLeg = Enumerable.Range(0, 40).Select(i => new FloatingCashflowDeal()
            {
                Notional = 1e6,
                Currency = Currency.EUR,
                StartDate = testDate.AddMonths(3 * i),
                EndDate = testDate.AddMonths(3 * (i + 1))
            });

            var correlationMatrix = context.Factory.CreateNArray(new double[,] {
                { 1.0,      -0.92,   0.5},
                { -0.92,     1.0,    -0.8},
                { 0.5,    -0.8,   1.0}
            });

            var identifiers = new string[] { "IR_DiscountFactor_EUR_Factor0", "IR_DiscountFactor_EUR_Factor1", 
                "IR_DiscountFactor_EUR_Factor2"};
            CorrelationHelper.AddMultivariateModelWeightsProvider(context, identifiers, correlationMatrix);

            pricers = fixedLeg.Select(d => new FixedCashflowPricer(d) as IPricer)
                .Concat(floatingLeg.Select(d => new FloatingCashflowPricer(d) as IPricer)).ToList();
            foreach (var pricer in pricers) pricer.Register(graph);

            var testVariates0 = graph.RegisterFactor<NormalVariates>("IR_DiscountFactor_EUR_Factor0");
            var testVariates1 = graph.RegisterFactor<NormalVariates>("IR_DiscountFactor_EUR_Factor1");
            var testVariates2 = graph.RegisterFactor<NormalVariates>("IR_DiscountFactor_EUR_Factor2");
            var testFactor = graph.RegisterModel<MeanRevertingNormalPathModel>("IR_DiscountFactor_EUR_Factor0");
            model = graph.RegisterModel<LinearGaussianModel>("EUR");
            var numeraire = graph.RegisterModel<NumeraireModel>("EUR");

            model.Factors[0].Sigma = 0.02; model.Factors[0].Lambda = 0.05;
            model.Factors[1].Sigma = 0.03; model.Factors[1].Lambda = 0.2;
            model.Factors[2].Sigma = 0.01; model.Factors[2].Lambda = 1.0;

            var years = new double[] { 0, 1/365.35, 7/365.25, 14/365.25, 1/12, 2/12, 0.25, 0.5, 0.75, 1, 1.5, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60 };
            var zeroRatesInPercent = new double[] { -0.3, -0.3, -0.29, -0.29, -0.3, -0.31, -0.31, -0.32, -0.32, -0.33, -0.34, -0.35, -0.35, -0.33, -0.28, -0.2, -0.11, -0.01, 0.1, 0.2, 0.37, 0.56, 0.71, 0.75, 0.76, 0.75, 0.75, 0.72, 0.7, 0.67, 0.64 };

            model.ZeroRatesT0 = new Curve(years.Zip(zeroRatesInPercent,
                (y, r) => new DataPoint(
                    testDate.AddDays(y * 365.25),
                    NArray.CreateScalar(r / 100)
                    )).ToArray());

            var runner = graph.ToSimulationRunner();
            runner.Prepare();
            runner.Simulate();

            timePoints = graph.Context.Settings.SimulationTimePoints;
            allZeroRatesT0 = model.ZeroRatesT0.Data.Select(d => d.Value);
        }
    }
}
