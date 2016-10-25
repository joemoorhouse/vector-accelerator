using System;
using System.Threading;
using VectorAccelerator.Tests;

namespace VectorAccelerator.Launcher
{
    class Program
    {     
        static void Main(string[] args)
        {
            var swapAADTest = new BasicSwapAADTest();
            swapAADTest.TestEndToEnd();


            //var mult = new MultiplyAggregateTests();
            //mult.SimpleTest();
        }
    }
}
