using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator.Tests;

namespace VectorAccelerator.Launcher
{
    class Program
    {
        static void Main(string[] args)
        {
            var test = new AcceleratorTestsCPU();
            test.SimpleSpeedTest();
            //test.TestMKLWithNETThreads();
            Console.ReadKey();
        }
    }
}
