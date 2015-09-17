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
            test4.GenericVersion();
            //test3.CumulativeNormalTest();

            //test2.OptionPricingTest();
            //test.TestBlackScholes();
            //test.SimpleSpeedTest();
            //test.TestMKLWithNETThreads();
            Console.ReadKey();
        }
    }
}
