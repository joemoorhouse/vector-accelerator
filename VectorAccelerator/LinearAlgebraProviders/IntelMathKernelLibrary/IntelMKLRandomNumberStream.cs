using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using VectorAccelerator.Distributions;

namespace VectorAccelerator.LinearAlgebraProviders
{
    public class IntelMKLRandomNumberStream : IDisposable
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

        public readonly IntPtr RandomStream;

        public IntelMKLRandomNumberStream(RandomNumberGeneratorType type, int seed)
        {
            RandomStream = new IntPtr();
            int status = -1;

            status = vslNewStream(ref RandomStream, (int)type, seed);
            //status = vslNewStream(ref randomStream, BRNGMapper[(int)BRNG.MRG32K3A], seed);

            if (status != 0) throw new Exception("Random number generation stream failed to initialise.");
        }

        public void Dispose()
        {
            DeleteStream();
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
            if (RandomStream != IntPtr.Zero)
            {
                DeleteStream(RandomStream);
            }
        }

        private void DeleteStream(IntPtr stream)
        {
            int status = vslDeleteStream(ref stream);
            if (status != 0) throw new Exception("failed to delete stream.");
        }

        public static int[] BRNGMapper;

        static IntelMKLRandomNumberStream()
        {
            NativeLibraryHelper.AddLibraryPath();

            BRNGMapper = new int[13];
            int increment = 1 << 20;
            int value = increment;
            for (int i = 0; i < 13; ++i)
            {
                BRNGMapper[i] = value;
                value += increment;
            }
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
    }
}
