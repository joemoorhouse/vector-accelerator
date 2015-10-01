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

namespace VectorAccelerator.Tests
{
    public class ThrowAwayTests
    {   
        [TestMethod]
        public void TestMKLWithNETThreads()
        {
            var random = new Random();
            int threads = 1;
            int dynamic = 0;
            int threading = 1;
            //IntelMathKernalLibrary.mkl_set_threading_layer(ref threading);
            //IntelMathKernalLibrary.mkl_set_dynamic(ref dynamic);
            IntelMathKernelLibrary.mkl_set_num_threads(ref threads);

            var x = new double[512];
            var y = new double[512];

            var options = new ParallelOptions();
            //options.MaxDegreeOfParallelism = 1;

            for (int i = 0; i < 512; ++i)
            {
                x[i] = random.NextDouble();
            }
            var provider = new IntelMKLLinearAlgebraProvider();
            var watch = new Stopwatch();
            watch.Start();
            for (int j = 0; j < 10000; ++j)
            {
                for (int i = 0; i < 20; ++i)
                {
                    for (int k = 0; k < 10; ++k)
                        IntelMathKernelLibrary.Exp(x, 0, y, 0, 256);
                }
            }
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            watch.Restart();
            for (int j = 0; j < 10000; ++j)
            {
                Parallel.For(0, 20, (i) =>
                {
                    for (int k = 0; k < 10; ++k)
                        IntelMathKernelLibrary.Exp(x, 0, y, 0, 256);
                });
            }
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            watch.Restart();
            watch.Start();
            for (int j = 0; j < 10000; ++j)
            {
                for (int i = 0; i < 20; ++i)
                {
                    for (int k = 0; k < 10; ++k)
                        IntelMathKernelLibrary.Exp(x, 0, y, 0, 256);
                }
            }
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            Console.ReadKey();


            var result = new double[10];


            //int threadingLayer = 0;
            //int layer = IntelMathKernalLibrary.mkl_set_threading_layer(ref threadingLayer);

            int size = 3000;
            var a = new double[size, size];
            var b = new double[size, size];
            var c = new double[size, size];

            for (int i = 0; i < size; ++i)
            {
                for (int j = 0; j < size; ++j)
                {
                    a[i, j] = random.NextDouble();
                    b[i, j] = random.NextDouble();
                }
            }

            threads = 2;

            watch = new Stopwatch();
            watch.Start();

            IntelMathKernelLibrary.mkl_set_num_threads(ref threads);
            IntelMathKernelLibrary.MatrixMultiply(a, b, c);

            Console.WriteLine(watch.ElapsedMilliseconds);

            watch.Restart();

            threads = 1;
            IntelMathKernelLibrary.mkl_set_num_threads(ref threads);
            IntelMathKernelLibrary.MatrixMultiply(a, b, c);

            Console.WriteLine(watch.ElapsedMilliseconds);

            watch.Restart();
            threads = 2;
            IntelMathKernelLibrary.mkl_set_num_threads(ref threads);
            IntelMathKernelLibrary.MatrixMultiply(a, b, c);

            Console.WriteLine(watch.ElapsedMilliseconds);

            var input1 = Enumerable.Range(0, 100000).Select(i => i / 100000.0).ToArray();
            var input2 = Enumerable.Range(0, 100000).Select(i => 2 * i / 100000.0).ToArray();
            foreach (var setting in new List<int> { 1, 2 })
            {
                options.MaxDegreeOfParallelism = setting;
                threads = setting;
                //int dynamic = 0;
                IntelMathKernelLibrary.mkl_set_num_threads(ref threads);
                //VectorAccelerator.LinearAlgebraProviders.IntelMathKernalLibrary.mkl_set_dynamic(ref dynamic);
                Console.WriteLine(IntelMathKernelLibrary.mkl_get_max_threads());
                Console.WriteLine(IntelMathKernelLibrary.mkl_get_dynamic());

                AcceleratorTestsCPU.Timeit(() =>
                {
                    for (int i = 0; i < 10; ++i)
                    //Parallel.For(0, 10, options, i =>
                    {
                        //result[i] = IntelMathKernalLibrary.Dot(
                        //    input1, 0, input2, 0, input1.Length);

                        IntelMathKernelLibrary.MatrixMultiply(a, b, c);

                        //double temp = 1;
                        //for (int j = i; j < 1000000; ++j)
                        //{
                        //    double x = j / 1000000.0;
                        //    temp += Math.Sin(x) * Math.Sin(x) + Math.Cos(x) * Math.Cos(x);
                        //}
                        //result[i] = temp;
                        //});
                    }
                }, 10, 1);
                Console.WriteLine(Environment.NewLine);
            }
        }
    }
}
