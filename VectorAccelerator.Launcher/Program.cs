using System;
using System.Diagnostics;
using VectorAccelerator.Tests;

namespace VectorAccelerator.Launcher
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Running demonstrations");

            RunDemonstrations();

            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }

        static void RunDemonstrations()
        {
            var presentationTests = new PresentationTests();
            Console.WriteLine("Running worked example test");
            presentationTests.WorkedExample();
            Console.WriteLine();

            Console.WriteLine("Running Black-Scholes test");
            presentationTests.BlackScholes();
            Console.WriteLine();

            Console.WriteLine("Running Black-Scholes performance test");
            presentationTests.BlackScholesPerformance();
            Console.WriteLine();

            Console.WriteLine("Running swap portfolio AAD calculation test");
            var swapAADTest = new BasicSwapAADTest();
            swapAADTest.TestEndToEnd();
            Console.WriteLine();
        }

        static void RunInDevelopment()
        {
            var test = new MultiplyAggregateTests();
            test.TestPerformance();
        }
    }
}
