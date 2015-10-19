using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator.Tests;
using VectorAccelerator.Tests.Checks;

namespace VectorAccelerator.Launcher
{
    class Program
    {
        static void Main(string[] args)
        {
            var test = new AcceleratorTestsCPU();
            var test2 = new SimpleCounterpartyRiskTest();
            var test3 = new CheckApplicationLevelThreadingMKL();
            var test4 = new ThrowAwayTests();
            var test5 = new DistributionTests();
            var test6 = new SpeedComparison();
            var test7 = new InterpolationTest();

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
