using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using VectorAccelerator.Distributions;

namespace VectorAccelerator.LinearAlgebraProviders
{
    public unsafe class IntelMathKernelLibraryRandom 
    {
        [DllImport(IntelMathKernelLibrary.DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdRngGaussian(int method, IntPtr stream, int length, double* vector, double mean, double sigma);

        [DllImport(IntelMathKernelLibrary.DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdRngGaussian(int method, IntPtr stream, int length, [In, Out] double[] vector, double mean, double sigma);

        [DllImport(IntelMathKernelLibrary.DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdRngGaussian(int method, IntPtr stream, int length, [In, Out] double[,] matrix, double mean, double sigma);

        [DllImport(IntelMathKernelLibrary.DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vsRngGaussian(int method, IntPtr stream, int length, [In, Out] float[,] matrix, float mean, float sigma);

        [DllImport(IntelMathKernelLibrary.DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdRngGaussian(int method, IntPtr stream, int length, [In, Out] ref double single, double mean, double sigma);

        [DllImport(IntelMathKernelLibrary.DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int viRngPoisson(int method, IntPtr stream, int length, [In, Out] int[] vector, double lambda);

        [DllImport(IntelMathKernelLibrary.DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int viRngPoisson(int method, IntPtr stream, int length, [In, Out] ref int single, double lambda);

        [DllImport(IntelMathKernelLibrary.DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdRngUniform(int method, IntPtr stream, int length, [In, Out] double[] vector, double a, double b);

        [DllImport(IntelMathKernelLibrary.DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdRngUniform(int method, IntPtr stream, int length, [In, Out] ref double single, double a, double b);

        [DllImport(IntelMathKernelLibrary.DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdRngExponential(int method, IntPtr stream, int length, [In, Out] double[] vector, double displacement, double rate);

        [DllImport(IntelMathKernelLibrary.DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int MKL_Set_Threading_Layer(int threading);

        [DllImport(IntelMathKernelLibrary.DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int MKL_Set_Num_Threads(int nThreads);

        static IntelMathKernelLibraryRandom()
        {
            NativeLibraryHelper.AddLibraryPath();
        }

        public static void FillNormals(double[] toFill, IntelMKLRandomNumberStream randomStream)
        {
            FillNormals(toFill, randomStream, 0, toFill.Length);
        }

        public static void FillNormals(double[] toFill, IntelMKLRandomNumberStream randomStream, int startIndex, int variateCount)
        {
            // method can be 0 - BoxMuller, 1 - BoxMuller2 or 3 - ICDF
            int status;

            fixed (double* p = &toFill[startIndex])
            {
                status = vdRngGaussian(1, randomStream.RandomStream, 
                    variateCount, p, 0, 1);
            }

            if (status != 0) throw new Exception("Normal number generation failed.");
        }

        //public void NormalVector(double[] normals, int variateCount)
        //{
        //    // method can be 0 - BoxMuller, 1 - BoxMuller2 or 3 - ICDF
        //    int status;

        //    status = vdRngGaussian(1, randomStream, variateCount, normals, 0, 1);
        //    if (status != 0) throw new Exception("Normal number generation failed.");
        //}

        //public double NextNormal()
        //{
        //    double normal = 0;
        //    vdRngGaussian(1, randomStream, 1, ref normal, 0, 1);
        //    return normal;
        //}

        //public int NextPoisson(double lambda)
        //{
        //    int poisson = 0;
        //    viRngPoisson(0, randomStream, 1, ref poisson, lambda);
        //    return poisson;
        //}

        //public void PoissonVector(int[] poissons, double lambda)
        //{
        //    int status;
        //    status = 0;
        //    status = viRngPoisson(0, randomStream, poissons.Length, poissons, lambda);
        //    if (status != 0) throw new Exception("Poisson number generation failed.");
        //}

        //public double NextUniform()
        //{
        //    double uniform = 0;
        //    vdRngUniform(0, randomStream, 1, ref uniform, 0, 1);
        //    return uniform;
        //}

        //public void UniformVector(double[] uniforms)
        //{
        //    // method can be 0 - BoxMuller, 1 - BoxMuller2 or 3 - ICDF
        //    int status;
        //    status = vdRngUniform(0, randomStream, uniforms.Length, uniforms, 0, 1);
        //    if (status != 0) throw new Exception("Uniform number generation failed.");
        //}

        //public void NormalMatrix(double[,] normals)
        //{
        //    // method can be 0 - BoxMuller, 1 - BoxMuller2 or 3 - ICDF
        //    int status = 0;
        //    status = vdRngGaussian(1, randomStream, normals.Length, normals, 0, 1);
        //    if (status != 0) throw new Exception("Normal number generation failed.");
        //}

        //public void NormalMatrix(float[,] normals)
        //{
        //    // method can be 0 - BoxMuller, 1 - BoxMuller2 or 3 - ICDF
        //    int status = 0;
        //    status = vsRngGaussian(1, randomStream, normals.Length, normals, 0f, 1f);
        //    if (status != 0) throw new Exception("Normal number generation failed.");
        //}
    }
}
