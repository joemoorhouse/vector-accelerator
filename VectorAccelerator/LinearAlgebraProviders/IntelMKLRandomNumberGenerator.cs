using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace VectorAccelerator.LinearAlgebraProviders
{
    public class IntelMKLRandomNumberGenerator : IRandomNumberGenerator
    {
        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vslNewStream(ref IntPtr stream, int brng, int seed);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vslNewStreamEx(ref IntPtr stream, int brng, int nparams, uint[] parameters);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vslDeleteStream(ref IntPtr stream);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdRngGaussian(int method, IntPtr stream, int length, [In, Out] double[] vector, double mean, double sigma);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdRngGaussian(int method, IntPtr stream, int length, [In, Out] double[,] matrix, double mean, double sigma);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vsRngGaussian(int method, IntPtr stream, int length, [In, Out] float[,] matrix, float mean, float sigma);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdRngGaussian(int method, IntPtr stream, int length, [In, Out] ref double single, double mean, double sigma);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int viRngPoisson(int method, IntPtr stream, int length, [In, Out] int[] vector, double lambda);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int viRngPoisson(int method, IntPtr stream, int length, [In, Out] ref int single, double lambda);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdRngUniform(int method, IntPtr stream, int length, [In, Out] double[] vector, double a, double b);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdRngUniform(int method, IntPtr stream, int length, [In, Out] ref double single, double a, double b);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdRngExponential(int method, IntPtr stream, int length, [In, Out] double[] vector, double displacement, double rate);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int MKL_Set_Threading_Layer(int threading);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int MKL_Set_Num_Threads(int nThreads);

        IntPtr randomStream = IntPtr.Zero;

        public IntelMKLRandomNumberGenerator(RandomNumberGeneratorType type, int seed)
        {
            CreateStream(type, seed);
        }

        public void Dispose()
        {
            DeleteStream();
        }

        public void CreateStream(RandomNumberGeneratorType type, int seed)
        {
            randomStream = new IntPtr();
            int status = -1;

            status = vslNewStream(ref randomStream, (int)type, seed);
            //status = vslNewStream(ref randomStream, BRNGMapper[(int)BRNG.MRG32K3A], seed);

            if (status != 0) throw new Exception("Random number generation stream failed to initialise.");
        }

        public void SetStream(IntPtr stream)
        {
            randomStream = stream;
        }

        public static IntPtr[] CreateMultithreadStreams(int seed, int nStreams)
        {
            IntPtr[] multithreadStreams = new IntPtr[nStreams];
            for (int i = 0; i < nStreams; ++i)
            {
                int status = -1;
                status = vslNewStream(ref multithreadStreams[i], BRNGMapper[(int)BRNG.MT2203] + i, seed);
                if (status != 0) throw new Exception("Random number generation stream failed to initialise.");
            }
            
            return multithreadStreams;
        }

        public void DeleteStream()
        {
            if (randomStream != IntPtr.Zero)
            {
                DeleteStream(randomStream);
                randomStream = IntPtr.Zero;
            }
        }

        private void DeleteStream(IntPtr stream)
        {
            int status = vslDeleteStream(ref stream);
            if (status != 0) throw new Exception("Failed to delete stream.");
        }

        /// <summary>
        /// Uses BoxMuller2 and MT19937 generator by default.
        /// </summary>
        /// <param name="length"></param>
        /// <returns>Array of uncorrelated normal random variates.</returns>
        public static double[] NormalVector(int length, int seed)
        {
            IntPtr stream = new IntPtr();
            int status;
            status = vslNewStream(ref stream, BRNGMapper[(int)BRNG.MT19937], seed);
            if (status != 0) throw new Exception("Random number generation stream failed to initialise.");
            double[] normals = new double[length];
            // method can be 0 - BoxMuller, 1 - BoxMuller2 or 3 - ICDF

            status = vdRngGaussian(1, stream, normals.Length, normals, 0, 1);
            if (status != 0) throw new Exception("Normal number generation failed.");
            
            return normals;
        }

        public void NormalVector(double[] normals)
        {
            // method can be 0 - BoxMuller, 1 - BoxMuller2 or 3 - ICDF
            int status;

            status = vdRngGaussian(1, randomStream, normals.Length, normals, 0, 1);
            if (status != 0) throw new Exception("Normal number generation failed.");          
        }

        public void NormalVector(double[] normals, int variateCount)
        {
            // method can be 0 - BoxMuller, 1 - BoxMuller2 or 3 - ICDF
            int status;

            status = vdRngGaussian(1, randomStream, variateCount, normals, 0, 1);
            if (status != 0) throw new Exception("Normal number generation failed.");
        }

        public double NextNormal()
        {
            double normal = 0;
            vdRngGaussian(1, randomStream, 1, ref normal, 0, 1);
            return normal;
        }

        public int NextPoisson(double lambda)
        {
            int poisson = 0;
            viRngPoisson(0, randomStream, 1, ref poisson, lambda);
            return poisson;
        }

        public void PoissonVector(int[] poissons, double lambda)
        {
            int status;
            status = 0;
            status = viRngPoisson(0, randomStream, poissons.Length, poissons, lambda);
            if (status != 0) throw new Exception("Poisson number generation failed.");
        }

        public double NextUniform()
        {
            double uniform = 0;
            vdRngUniform(0, randomStream, 1, ref uniform, 0, 1);
            return uniform;
        }

        public void UniformVector(double[] uniforms)
        {
            // method can be 0 - BoxMuller, 1 - BoxMuller2 or 3 - ICDF
            int status;
            status = vdRngUniform(0, randomStream, uniforms.Length, uniforms, 0, 1);
            if (status != 0) throw new Exception("Uniform number generation failed.");
        }

        public void NormalMatrix(double[,] normals)
        {
            // method can be 0 - BoxMuller, 1 - BoxMuller2 or 3 - ICDF
            int status = 0;
            status = vdRngGaussian(1, randomStream, normals.Length, normals, 0, 1);
            if (status != 0) throw new Exception("Normal number generation failed.");
        }

        public void NormalMatrix(float[,] normals)
        {
            // method can be 0 - BoxMuller, 1 - BoxMuller2 or 3 - ICDF
            int status = 0;
            status = vsRngGaussian(1, randomStream, normals.Length, normals, 0f, 1f);
            if (status != 0) throw new Exception("Normal number generation failed.");
        }

        public static int[] PoissonVector(int length, double lambda, int seed)
        {
            IntPtr stream = new IntPtr();
            int status;
            status = vslNewStream(ref stream, BRNGMapper[(int)BRNG.MT19937], seed);
            if (status != 0) throw new Exception("Random number generation stream failed to initialise.");
            int[] poissons = new int[length];
            // method can be 0 - BoxMuller, 1 - BoxMuller2 or 3 - ICDF
            status = viRngPoisson(0, stream, length, poissons, lambda);
            if (status != 0) throw new Exception("Normal number generation failed.");
            return poissons;
        }

        public static double[] UniformVector(int length, double leftBound, double rightBound, int seed)
        {
            IntPtr stream = new IntPtr();
            int status;
            status = vslNewStream(ref stream, BRNGMapper[(int)BRNG.MT19937], seed);
            if (status != 0) throw new Exception("Random number generation stream failed to initialise.");
            double[] uniforms = new double[length];
            // method can be 0 - BoxMuller, 1 - BoxMuller2 or 3 - ICDF
            status = vdRngUniform(0, stream, length, uniforms, leftBound, rightBound);
            if (status != 0) throw new Exception("Normal number generation failed.");
            return uniforms;
        }

        /// <summary>
        /// Uses BoxMuller2 and MT19937 generator by default.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        /// <param name="seed"></param>
        /// <returns>2D array of uncorrelated normal random variates.</returns>
        public static double[,] NormalMatrix(int rows, int columns, int seed)
        {
            IntPtr stream = new IntPtr();
            int status;
            status = vslNewStream(ref stream, BRNGMapper[(int)BRNG.MT19937], seed);
            if (status != 0) throw new Exception("Random number generation stream failed to initialise.");
            double[,] normals = new double[rows, columns];
            // method can be 0 - BoxMuller, 1 - BoxMuller2 or 3 - ICDF
            status = vdRngGaussian(1, stream, rows * columns, normals, 0, 1);
            if (status != 0) throw new Exception("Normal number generation failed.");
            return normals;
        }

        // Random number generators:
        public enum BRNG
        {
            MCG31, R250, MRG32K3A, MCG59, WH, SOBOL, NIEDERR, MT19937, MT2203, IABSTRACT,
            DABSTRACT, SABSTRACT, SFMT19937
        }
        // Respectively:
        // MCG31: A 31-bit multiplicative congruential generator.
        // R250: A generalized feedback shift register generator.
        // MRG32K3A: A combined multiple recursive generator with two components of order 3.
        // MCG59: A 59-bit multiplicative congruential generator.
        // WH: A set of 273 Wichmann-Hill combined multiplicative congruential generators.
        // MT19937: A Mersenne Twister pseudorandom number generator.
        // MT2203: A set of 1024 Mersenne Twister pseudorandom number generators.
        // SFMT19937: A SIMD-oriented Fast Mersenne Twister pseudorandom number generator.
        // SOBOL: A 32-bit Gray code-based generator producing low-discrepancy sequences for dimensions 1 = s = 40; user-defined dimensions are also available. 
        // NIEDERR: A 32-bit Gray code-based generator producing low-discrepancy sequences for dimensions 1 = s = 318; user-defined dimensions are also available.
        // VSL_BRNG_IABSTRACT: An abstract random number generator for integer arrays.
        // VSL_BRNG_DABSTRACT: An abstract random number generator for double precision floating-point arrays.
        // VSL_BRNG_SABSTRACT: An abstract random number generator for single precision floating-point arrays.

        public static int[] BRNGMapper;

        static IntelMKLRandomNumberGenerator()
        {
            BRNGMapper = new int[13];
            int increment = 1 << 20;
            int value = increment;
            for (int i = 0; i < 13; ++i)
            {
                BRNGMapper[i] = value;
                value += increment;
            }
        }
    }
}
