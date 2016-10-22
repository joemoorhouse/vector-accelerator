using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RiskEngine.Framework;
using RiskEngine.Calibration;
using RiskEngine.Models;
using VectorAccelerator;
using VectorAccelerator.Distributions;
using RiskEngine.Factors;

namespace RiskEngine.Tests
{    
    [TestClass]
    public class BasicModelTests
    {
        [TestMethod]
        public void NearestCorrelation()
        {
            var factory = new NArrayFactory(StorageLocation.Host);

            var correlation = factory.CreateNArray(new double[,] {
            { 1.0,      0.84,   -0.42},
            { 0.84,     1.0,    0.14},
            { -0.42,    0.14,   1.0}
            });

            var nearestCorrelation = CorrelationHelper.NearestCorrelationMatrix(correlation);
            
            Console.WriteLine(nearestCorrelation.ToString());
        }

        [TestMethod]
        public void MeanRevertingModel()
        {
            var testDate = new DateTime(2015, 12, 1);

            var graph = new SimulationGraph(StorageLocation.Host, testDate);

            var identifiers = CreateTestWeightsProvider(graph.Context, WeightsType.Cholesky);

            var factor = graph.RegisterFactor<MeanRevertingNormal>(identifiers[0]);

            var runner = graph.ToSimulationRunner();

            // prepare to simulate
            runner.Prepare();

            var intervals = graph.Context.Settings.SimulationIntervals;

            var percentiles = new double[] { 1, 10, 50, 90, 99 };
            var result = new SimulationResult(percentiles.Select(p => new PercentileMeasure(p)));
            // simulate to the first time point; factors will hold values used to simulate across the specified interval 

            VectorAccelerator.Plot.PlotHelper.QuickPlot(new double[] { 1, 2, 3 }, new double[] { 1.2, 5.6, 2.3 });
            
            while (runner.StepNext())
            {
                var values = NMath.Percentiles(factor.Value, percentiles).ToList();
            }
        }

        [TestMethod]
        public void MultiVariateNormalModel()
        {
            var testDate = new DateTime(2015, 12, 1);

            var graph = new SimulationGraph(StorageLocation.Host, testDate);

            int weightsCount = 1000;

            var identifiers = CreateTestWeightsProvider(graph.Context, WeightsType.Returns, weightsCount);

            var factor1 = graph.RegisterFactor<NormalVariates>(identifiers[0]);
            var factor2 = graph.RegisterFactor<NormalVariates>(identifiers[1]);
            var factor3 = graph.RegisterFactor<NormalVariates>(identifiers[2]);

            var runner = graph.ToSimulationRunner();
         
            // prepare to simulate
            runner.Prepare();

            var intervals = graph.Context.Settings.SimulationIntervals;
            
            // simulate to the first time point; factors will hold values used to simulate across the specified interval 
            runner.Step(intervals.First());

            var check = NMath.Correlation(factor1.Value, factor2.Value);

            Assert.AreEqual(Math.Round(check, 2), 0.84);
        }

        private enum WeightsType { Returns, Cholesky };

        private List<string> CreateTestWeightsProvider(Context context,
            WeightsType weightsType, int returnsCount = 1000)
        {
            var correlationMatrix = context.Factory.CreateNArray(new double[,] {
                { 1.0,      0.84,   -0.42},
                { 0.84,     1.0,    0.14},
                { -0.42,    0.14,   1.0}
            });
            correlationMatrix = CorrelationHelper.NearestCorrelationMatrix(correlationMatrix);
            
            int weightsCount;
            NArray weightsMatrix;
            if (weightsType == WeightsType.Returns)
            {
                weightsCount = returnsCount;
                weightsMatrix = CalculateSyntheticReturns(correlationMatrix, weightsCount);
            }
            else
            {
                weightsCount = correlationMatrix.RowCount;
                weightsMatrix = NMath.CholeskyDecomposition(correlationMatrix);
            }

            var weights = context.Data.AddCalibrationParametersProvider
                (new WeightsProvider(weightsCount));

            var identifiers = Enumerable.Range(1, weightsMatrix.ColumnCount)
                .Select(i => string.Format("TestFactor{0}", i)).ToList();

            for (int i = 0; i < weightsMatrix.ColumnCount; ++i)
            {
                weights.AddValue(identifiers[i], weightsMatrix.Column(i));
            }

            return identifiers;
        }

        private NArray CalculateSyntheticReturns(NArray correlationMatrix, int returnsCount)
        {
            var location = StorageLocation.Host;
            var cholesky = NMath.CholeskyDecomposition(correlationMatrix);
            var variates = new NArray(location, returnsCount, correlationMatrix.RowCount);
            using (var stream = new RandomNumberStream(location))
            {
                var normal = new Normal(stream, 0, 1);
                variates.FillRandom(normal);
            }
            return variates * cholesky.Transpose();        
        }

        public interface ICurve
        {
            double this[int simId, int timeId, int x] 
            {
                get; set;

            }
        }

        int[] tenorDays = new int[] { 0, 1, 2, 3, 4, 20, 30, 365, 3650, 36500 };
        ICurve logDF;

        public double GetRate(int simId, int timeId, double x)
        {
            return GetRate(simId, timeId, (int)Math.Round(x * 365.35));
        }

        public double GetRate(int simId, int timeId, int xDays)
        {
            int left = 0;
            int right = tenorDays.Length + 1;

            while (right - left > 1)
            {
                int index = (left + right) >> 1;
                if (xDays >= tenorDays[index])
                {
                    left = index;
                }
                else
                {
                    right = index;
                }
            }

            double time = (double)xDays;
            double leftTime = (double)tenorDays[left];
            double rightTime = (double)tenorDays[right];
            double weightLeft = (rightTime - time) / (rightTime - leftTime);
            double leftValue = -logDF[simId, timeId, left];
            double rightValue = -logDF[simId, timeId, right];

            return weightLeft * leftValue / leftTime + (1 - weightLeft) * rightValue / rightTime;
        }

        public double GetDiscountFactor(int simId, int timeId, int xDays)
        {
            int left = 0;
            int right = tenorDays.Length + 1;

            while (right - left > 1)
            {
                int index = (left + right) >> 1;
                if (xDays >= tenorDays[index])
                {
                    left = index;
                }
                else
                {
                    right = index;
                }
            }

            double time = (double)xDays;
            double leftTime = (double)tenorDays[left];
            double rightTime = (double)tenorDays[right];
            double weightLeft = (rightTime - time) / (rightTime - leftTime);
            double leftValue = logDF[simId, timeId, left];
            double rightValue = logDF[simId, timeId, right];

            return Math.Exp
                ((weightLeft * leftValue / leftTime
                + (1 - weightLeft) * rightValue / rightTime) * time);
        }

    }
}
