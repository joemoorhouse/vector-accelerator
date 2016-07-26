using System;
using System.Linq;
using System.Text;
using VectorAccelerator;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VectorAccelerator.NArrayStorage;
using System.Threading.Tasks;
using VectorAccelerator.Distributions;
using VectorAccelerator.LinearAlgebraProviders;
using RiskEngine.Framework;
using RiskEngine.Pricers;
using RiskEngine.Calibration;
using RiskEngine.Factors;
using RiskEngine.Models;
using RiskEngine.Data;

namespace VectorAccelerator.Tests
{
    public class BasicSwapAADTest
    {
               
        public void SimulateAll()
        {
            LinearGaussianModel model;
            IEnumerable<NArray> allDiscountFactorT0;
            TimePoint[] timePoints;
            SimulationGraph graph;
            List<IPricer> pricers;

            SimulateModel(out model, out allDiscountFactorT0, out timePoints, out graph, out pricers);

            var simulationCount = graph.Context.Settings.SimulationCount;
            var timePointCount = graph.Context.Settings.SimulationTimePoints.Length;

            var resultStorage = 
                Enumerable.Range(0, timePointCount).Select(i =>
                Enumerable.Range(0, 1 + allDiscountFactorT0.Count()).
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
            VectorAccelerator.Tests.TestHelpers.Timeit(() =>
            {
                NArray.Evaluate(() =>
                {
                    NArray pv;
                    pricers.First().Price(10, out pv);
                    return pv;
                },
                    new List<NArray>(), Aggregator.ElementwiseAdd, tempStorage);
            }, 10, 10);

            Console.WriteLine(); Console.WriteLine(string.Format("Deferred execution, {0} flows, no derivatives, single thread", pricers.Count));
            VectorAccelerator.Tests.TestHelpers.Timeit(() =>
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
            }, 1, 1);

            var percentiles = new double[] { 1, 10, 50, 90, 99 };
            var measures = new List<IList<double>>();
            for (int i = 0; i < timePoints.Length; ++i)
            {
                var values = NMath.Percentiles(resultStorageSingle[i].First(), percentiles).ToList();
                measures.Add(values);
            }

            var times = timePoints.Select(p => p.YearsFromBaseDate).ToArray();
            var profile10 = measures.Select(p => p[1]).ToArray();
            var profile90 = measures.Select(p => p[3]).ToArray();

            VectorAccelerator.Plot.PlotHelper.QuickPlot(times, profile90);
            //return;

            Console.WriteLine(); Console.WriteLine(string.Format("Deferred execution, {0} flows, {1} derivatives, multiple threads", pricers.Count, allDiscountFactorT0.Count()));
            VectorAccelerator.Tests.TestHelpers.Timeit(() =>
            {
                Calculations.Calculate(graph, pricers);
                //Calculations.Calculate(graph, pricers, allDiscountFactorT0.ToList());
            }, 1, 1);

            Console.WriteLine(); Console.WriteLine(string.Format("Deferred execution, {0} flows, {1} derivatives, single thread", pricers.Count, allDiscountFactorT0.Count()));
            VectorAccelerator.Tests.TestHelpers.Timeit(() =>
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
                        allDiscountFactorT0.ToArray(), Aggregator.ElementwiseAdd, resultStorage[i]);
                    }
                }
            }, 1, 1);
        }
        
        public void CheckBasics()
        {
            LinearGaussianModel model;
            IEnumerable<NArray> allDiscountFactorT0;
            TimePoint[] timePoints;
            SimulationGraph graph;
            List<IPricer> pricers;

            SimulateModel(out model, out allDiscountFactorT0, out timePoints, out graph, out pricers);
            CheckBasics(model, allDiscountFactorT0, timePoints);
        }

        public void PerformanceTest()
        {
            LinearGaussianModel model;
            IEnumerable<NArray> allDiscountFactorT0;
            TimePoint[] timePoints;
            SimulationGraph graph;
            List<IPricer> pricers;

            SimulateModel(out model, out allDiscountFactorT0, out timePoints, out graph, out pricers);

            var storage = new NArray(StorageLocation.Host, 5000, 1);

            for (int i = 0; i < 20; ++i)
            {
                var result3 = NArray.Evaluate(() =>
                {
                    var df = model.ForwardRate(10, timePoints[10].DateTime.AddMonths(3), timePoints[10].DateTime.AddMonths(6));
                    return df;
                }, new List<NArray>(), Aggregator.ElementwiseAdd, new NArray[] { storage }); // allDiscountFactorT0.ToArray()
            }
        }

        public void CheckPerformance()
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
       

        public void SimulateModel(out LinearGaussianModel model, out IEnumerable<NArray> allDiscountFactorT0,
            out TimePoint[] timePoints, out SimulationGraph graph, out List<IPricer> pricers)
        {
            var testDate = new DateTime(2015, 12, 1);

            graph = new SimulationGraph(StorageLocation.Host, testDate);
            var context = graph.Context;
            context.Settings.SimulationCount = 5000;

            var fixedLeg = Enumerable.Range(0, 40).Select(i => new FixedCashflowDeal()
                {
                    Notional = -1e6,
                    Rate = 0.0078,
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
                { 1.0,      -0.947,   0.528},
                { -0.947,     1.0,    -0.767},
                { 0.528,    -0.767,   1.0}
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

            model.Factors[0].Sigma = 0.0197; model.Factors[0].Lambda = 0.05;
            model.Factors[1].Sigma = 0.0268; model.Factors[1].Lambda = 0.188;
            model.Factors[2].Sigma = 0.0114; model.Factors[2].Lambda = 0.957;

            var years = new double[] { 0.1, 0.25, 0.5, 0.75, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 15, 20, 25, 40, 60 };
            var zeroRatesInPercent = new double[] { -0.25, -0.25, -0.25, -0.25, -0.25, -0.25, -0.25 - 0.18, -0.08, 0.03, 0.16, 0.3, 0.4, 0.55, 0.75, 1, 1.15, 1.2, 1.2, 1.2 };
            model.DiscountFactorT0 = new Curve(years.Zip(zeroRatesInPercent,
                (y, r) => new DataPoint(
                    testDate.AddDays(y * 365.25), 
                    NArray.CreateScalar(Math.Exp(-y * r / 100))
                    )).ToArray());

            var runner = graph.ToSimulationRunner();
            runner.Prepare();
            runner.Simulate();

            timePoints = graph.Context.Settings.SimulationTimePoints;
            allDiscountFactorT0 = model.DiscountFactorT0.Data.Select(d => d.Value);

            bool checkProfile = true;
            if (checkProfile)
            {
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

            bool checkForwardRate = true;
            if (checkForwardRate)
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
        }

        public void CheckBasics(LinearGaussianModel model, IEnumerable<NArray> allDiscountFactorT0,
            TimePoint[] timePoints)
        {

            bool checkResults = true;
            if (checkResults)
            {
                var result1 = NArray.Evaluate(() =>
                {
                    var df = model[10, timePoints[10].DateTime.AddMonths(3)];
                    return df;
                }, model.DiscountFactorT0[3].Value);

                var unbumped = model[10, timePoints[10].DateTime.AddMonths(3)];
                var expected = unbumped.DebugDataView.ToArray();

                var obtained = result1[0].DebugDataView.ToArray();

                Assert.IsTrue(TestHelpers.AgreesAbsolute(expected, obtained));

                var log = new StringBuilder();
                var result2 = NArray.Evaluate(() =>
                {
                    var df = model[10, timePoints[10].DateTime.AddMonths(3)];
                    return df;
                }, log, allDiscountFactorT0.ToArray());

                var logCheck = log.ToString();

                var obtained2 = result2[0].DebugDataView.ToArray();
                var obtained2_Deriv = result2[1].DebugDataView.ToArray();

                model.DiscountFactorT0.Data[0] = new DataPoint(model.DiscountFactorT0.Data[0].Time, model.DiscountFactorT0.Data[0].Value + 1e-6);

                var bumped = model[10, timePoints[10].DateTime.AddMonths(3)];

                var expected_deriv = ((bumped - unbumped) / 1e-6)
                    .DebugDataView.ToArray();

                Assert.IsTrue(TestHelpers.AgreesAbsolute(expected, obtained2));
                Assert.IsTrue(TestHelpers.AgreesAbsolute(expected_deriv, obtained2_Deriv, 1e-5));

                var result3 = NArray.Evaluate(() =>
                {
                    var df = model.ForwardRate(10, timePoints[10].DateTime.AddMonths(3), timePoints[10].DateTime.AddMonths(6));
                    return df;
                }, allDiscountFactorT0.ToArray());

                var expected3 = model.ForwardRate(10, timePoints[10].DateTime.AddMonths(3), timePoints[10].DateTime.AddMonths(6))
                    .DebugDataView.ToArray();
                var obtained3 = result3[0].DebugDataView.ToArray();

                Assert.IsTrue(TestHelpers.AgreesAbsolute(expected3, obtained3));
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
                        }, allDiscountFactorT0.ToArray());
                    }, 10, 10);
            }

            //VectorAccelerator.Plot.PlotHelper.QuickPlot(new double[] { 1, 2, 3 }, new double[] { 1.2, 5.6, 2.3 });
        }

        public IList<double[]> CalculateProfiles(IPath path, double[] percentiles)
        {
            return null;
        }
    }
}
