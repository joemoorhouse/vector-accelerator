using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskEngine.Tests;
using VectorAccelerator.Tests;
using VectorAccelerator.Tests.Checks;
using VectorAccelerator.Tests.Financial.CounterpartyCreditRisk;
using Random = System.Random;
using System.Diagnostics;

namespace VectorAccelerator.Launcher
{
    class Program
    {
        static void Main(string[] args)
        {
            var basicModelTests = new BasicModelTests();
            basicModelTests.MultiVariateNormalModel();
            basicModelTests.MeanRevertingModel();

            Console.ReadKey();
            return;

            //basicModelTests.GetRate(0, 0, 400 / 365.25);
            
            var test = new AcceleratorTestsCPU();
            var test2 = new SimpleCounterpartyRiskTest();
            var test3 = new CheckApplicationLevelThreadingMKL();
            var test4 = new ThrowAwayTests();
            var test5 = new DistributionTests();
            var test6 = new SpeedComparison();
            var test7 = new InterpolationTest();

            var test8 = new InterestRateModel();
            var test9 = new LinearAlgebraTests();

            test9.TestCorrelation();

            test8.CheckHullWhite();

            test7.VectorBinarySearch();

            // good tests:
            //test3.TranscendentalFunctionTest();
            test.TestBlackScholes();
            //test2.OptionPricingTest();



            //test6.OptionPricingTest();
            //test5.TestRandomNumberGeneration();
            

            //test.TestBlackScholes();
            //test.SimpleSpeedTest();
            //test.TestMKLWithNETThreads();
            Console.ReadKey();
        }
    }
}
