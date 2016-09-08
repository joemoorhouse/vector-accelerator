using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RiskEngine.Tests;
using VectorAccelerator.Tests;
using VectorAccelerator.Tests.Checks;
using VectorAccelerator.Tests.Financial.CounterpartyCreditRisk;
using Random = System.Random;
using System.Diagnostics;
using System.Linq.Expressions;

namespace VectorAccelerator.Launcher
{
    class Program
    {     
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-UK");

            //double a, b;
            //var test = Expression.Block(
            //    Expression.Assign(Expression.Parameter(typeof(double), "y"),
            //        Expression.Add(
            //            Expression.Parameter(typeof(double), "a"),
            //            Expression.Parameter(typeof(double), "b"))
            //            )
            //    );

            //var test2 = new CheckApplicationLevelThreadingMKL();
            //test2.TranscendentalFunctionTest();

            //var test2 = new PureTranscendentalWork();
            //test2.SimpleSpeedTest();

            //var illustrative = new Illustrative();
            //illustrative.Example1();

            var swapAADTest = new BasicSwapAADTest();
            swapAADTest.TestBasics();
            swapAADTest.TestEndToEnd();

            //var test3 = new AcceleratorTestsCPU();
            //test3.SimpleSpeedTest();

            //var sanity = new BasicCheckVectorPerformance();
            //sanity.CheckExponentialPerformance();

            //extendedExpressionBuilding.PerformanceTest();

            //extendedExpressionBuilding.CheckBasics();
            //extendedExpressionBuilding.CheckPerformance();

            //var expressionBuilding = new BasicExpressionBuildingTests();
            //expressionBuilding.AAD();

            //var testPrinciple = new CheckApplicationLevelThreadingMKL();
            //testPrinciple.VectorFundamentalsTest();
            
            //Console.ReadKey();


            //testPrinciple.TranscendentalFunctionTest();


            //var basicDifferentiationTests = new BasicDifferentiationTests();
            //basicDifferentiationTests.TestDivision();

            //var test0 = new CheckApplicationLevelThreadingMKL();
            //test0.TranscendentalFunctionTest();
            
            //expressionBuilding.CUDA();

            //var basicModelTests = new BasicModelTests();
            //basicModelTests.MultiVariateNormalModel();

            //var param1 = Expression.Parameter(typeof(double), "a");
            //var param2 = Expression.Parameter(typeof(double), "a");
            //var param3 = Expression.Parameter(typeof(double), "c");

            //var add = Expression.Assign(param3, Expression.Add(param1, param2));

            //var block = Expression.Block(add);




            //basicModelTests.MeanRevertingModel();

            //Console.ReadKey();
            //return;

            //basicModelTests.GetRate(0, 0, 400 / 365.25);
            
            //var test = new AcceleratorTestsCPU();
            //var test2 = new SimpleCounterpartyRiskTest();
            //var test3 = new CheckApplicationLevelThreadingMKL();
            //var test4 = new ThrowAwayTests();
            //var test5 = new DistributionTests();
            //var test6 = new SpeedComparison();
            //var test7 = new InterpolationTest();

            //var test8 = new InterestRateModel();
            //var test9 = new LinearAlgebraTests();

            //test6.OptionPricingTest();

            //test2.OptionPricingTest();


            //test9.TestCorrelation();
            //test8.CheckHullWhite();
            //test7.VectorBinarySearch();

            // good tests:
            //test3.TranscendentalFunctionTest();
            //test.TestBlackScholes();
            //test2.OptionPricingTest();



            //test6.OptionPricingTest();
            //test5.TestRandomNumberGeneration();
            

            //test.TestBlackScholes();
            //test.SimpleSpeedTest();
            //test.TestMKLWithNETThreads();
            Console.ReadKey();
        }

        public static void Example()
        {

        }


    }



    //public class OrnsteinUhlenbeckVariates
    //{
    //    public void PreGenerate()
    //    {
    //        for (int i = 0; i < timePoints.Length - 1; ++i)
    //        {
    //            double tiplus1 = timePoints[i + 1];
    //            int start = bridgingVariatesStartIndex[i];
    //            int stop = bridgingVariatesStopIndex[i];
    //            for (int j = start; j < stop; ++j)
    //            {
    //                double tj = bridgedTimePoints[j];
    //                double tjplus1 = bridgedTimePoints[j + 1];

    //                w1[j] = Math.Sqrt((tjplus1 - tj) / (tiplus1 - tj));
    //                //w2[j] = Math.Sqrt(())
    //                sumWeights[i] = Math.Sqrt(
    //                    (1 / (2 * a)) * Math.Exp(-2 * a * (tiplus1 - tjplus1))
    //                    * (1 - Math.Exp(-2 * a * (tjplus1 - tj)))
    //                    );

    //            }
    //        }
    //    }

    //    public double Get(int timeIndex)
    //    {
    //        double epsilon = variates[timeIndex];
    //        int start = bridgingVariatesStartIndex[timeIndex];
    //        int stop = bridgingVariatesStopIndex[timeIndex];
    //        double pathStart = 0;
    //        double integral = 0;
    //        for (int j = start; j < stop; ++j)
    //        {
    //            double epsilonPrime = variates[363 + j];
    //            double epsilonBridged = w1[j] * (epsilon - pathStart) + w2[j] * epsilonPrime;
    //            pathStart = pathStart + epsilonBridged;
    //            integral += sumWeights[j] * epsilonBridged;
    //        }
    //        return integral;
    //    }
    //}
}
