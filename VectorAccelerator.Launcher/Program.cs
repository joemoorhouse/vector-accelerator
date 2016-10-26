using System;
using System.Threading;
using VectorAccelerator.Tests;

namespace VectorAccelerator.Launcher
{
    class Program
    {     
        static void Main(string[] args)
        {
            Console.WriteLine("Running demonstrations");
            RunDemonstrations();
        }

        static void RunDemonstrations()
        {
            var presentationTests = new PresentationTests();
            Console.WriteLine("Running worked example test");
            presentationTests.WorkedExample();
            Console.WriteLine("Running Black-Scholes test");
            presentationTests.BlackScholes();

            Console.WriteLine();
            Console.WriteLine("Running swap portfolio AAD calculation test");
            var swapAADTest = new BasicSwapAADTest();
            swapAADTest.TestEndToEnd();

            Console.WriteLine("Any key to end");
            Console.ReadKey();
        }

        static void RunInDevelopment()
        {
            var test = new MultiplyAggregateTests();
            test.SimpleTest();
        }
    }
}
