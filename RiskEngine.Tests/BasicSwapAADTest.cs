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
        public void AADSwapPortfolio()
        {
            var testDate = new DateTime(2015, 12, 1);

            var graph = new SimulationGraph(StorageLocation.Host, testDate);
            var context = graph.Context;

            var fixedLeg = Enumerable.Range(0, 12).Select(i => new FixedCashflowDeal()
                {
                    Amount = 1e6,
                    Currency = Currency.EUR,
                    PaymentDate = testDate.AddMonths(3 * (i + 1))
                });

            var correlationMatrix = context.Factory.CreateNArray(new double[,] {
                { 1.0,      -0.947,   0.528},
                { -0.947,     1.0,    -0.767},
                { 0.528,    -0.767,   1.0}
            });
            var identifiers = new string[] { "IR_DiscountFactor_EUR_Factor0", "IR_DiscountFactor_EUR_Factor1", 
                "IR_DiscountFactor_EUR_Factor2"};
            CorrelationHelper.AddMultivariateModelWeightsProvider(context, identifiers, correlationMatrix);

            var pricers = fixedLeg.Select(d => new FixedCashflowPricer(d)).ToList();
            foreach (var pricer in pricers) pricer.Register(graph);

            var testVariates0 = graph.RegisterFactor<NormalVariates>("IR_DiscountFactor_EUR_Factor0");
            var testVariates1 = graph.RegisterFactor<NormalVariates>("IR_DiscountFactor_EUR_Factor1");
            var testVariates2 = graph.RegisterFactor<NormalVariates>("IR_DiscountFactor_EUR_Factor2");
            var testFactor = graph.RegisterModel<MeanRevertingNormalPathModel>("IR_DiscountFactor_EUR_Factor0");
            var model = graph.RegisterModel<LinearGaussianModel>("EUR");

            model.Factors[0].Sigma = 0.0197; model.Factors[0].Lambda = 0.05;
            model.Factors[1].Sigma = 0.0268; model.Factors[1].Lambda = 0.188;
            model.Factors[2].Sigma = 0.0114; model.Factors[2].Lambda = 0.957;

            var years = new double[] { 0.1, 3, 4, 5, 6, 7, 8, 9, 10, 12, 15, 20, 25, 40, 60 };
            var zeroRatesInPercent = new double[] { -0.25, -0.25, -0.18, -0.08, 0.03, 0.16, 0.3, 0.4, 0.55, 0.75, 1, 1.15, 1.2, 1.2, 1.2 };
            model.DiscountFactorT0 = new Curve(years.Zip(zeroRatesInPercent,
                (y, r) => new DataPoint(
                    testDate.AddDays(y * 365.25), 
                    NArray.CreateScalar(Math.Exp(-y * r / 100))
                    )).ToArray());

            var runner = graph.ToSimulationRunner();
            runner.Prepare();
            runner.Simulate();

            var check = Descriptive.Correlation(testVariates1.Value, testVariates2.Value);

            // check 3M rolling tenor
            var timePoints = graph.Context.Settings.SimulationTimePoints;
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

            var allDiscountFactorT0 = model.DiscountFactorT0.Data.Select(d => d.Value);
            
            var result1 = NArray.Evaluate(() =>
            {
                var df = model[10, timePoints[10].DateTime.AddMonths(3)];
                return df;
            }, model.DiscountFactorT0[3].Value);

            var unbumped = model[10, timePoints[10].DateTime.AddMonths(3)];
            var expected = unbumped.DebugDataView.ToArray();

            var obtained = result1[0].DebugDataView.ToArray();

            var result2 = NArray.Evaluate(() =>
            {
                var df = model[10, timePoints[10].DateTime.AddMonths(3)];
                return df;
            }, allDiscountFactorT0.ToArray());

            var obtained2 = result2[0].DebugDataView.ToArray();
            var obtained2_Deriv = result2[1].DebugDataView.ToArray();

            model.DiscountFactorT0.Data[0] = new DataPoint(model.DiscountFactorT0.Data[0].Time, model.DiscountFactorT0.Data[0].Value + 1e-6);

            var bumped = model[10, timePoints[10].DateTime.AddMonths(3)];

            var expected_deriv = ((bumped - unbumped) / 1e-6)
                .DebugDataView.ToArray();

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

            //VectorAccelerator.Plot.PlotHelper.QuickPlot(new double[] { 1, 2, 3 }, new double[] { 1.2, 5.6, 2.3 });

            Console.ReadKey();
        }

        public IList<double[]> CalculateProfiles(IPath path, double[] percentiles)
        {
            return null;
        }
    }
}
