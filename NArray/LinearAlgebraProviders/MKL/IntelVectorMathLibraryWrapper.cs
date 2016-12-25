using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NArray.LinearAlgebraProviders
{
    public class IntelMathKernelLibrary
    {
        public const string DllName = "mkl_vector_accelerator.dll";
    }

    public unsafe static class IntelVectorMathLibraryWrapper
    {
        public const string DllName = "mkl_vector_accelerator.dll"; //"mkl_rt.dll";

        #region Function DllImports

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdAdd(int n,
            double* left, double* right, double* result);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdSub(int n,
            double* left, double* right, double* result);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdMul(int n,
            double* left, double* right, double* result);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdDiv(int n,
            double* left, double* right, double* result);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdInv(int n,
            double* left, double* result);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdExp(int n,
            double* left, double* result);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdLn(int n,
            double* left, double* result);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdSqrt(int n,
            double* left, double* result);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdInvSqrt(int n,
            double* left, double* result);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdCdfNorm(int n,
            double* left, double* result);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdCdfNormInv(int n,
            double* left, double* result);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdLinearFrac(int n,
            double* left, double* right, double scale_a, double shift_a, double scale_b, double shift_b, double* result);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int cblas_daxpy(int n,
            double left, double* x, int inc_x, double[] result, int inc_y);

        [DllImport(DllName, EntryPoint = "LAPACKE_dlasrt", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int LAPACKE_dlasrt(char id, int n, double* d);

        #endregion

        #region Settings DllImports

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        public static extern int vmlSetMode(ref int mode);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        public static extern void mkl_set_num_threads(ref int n);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        public static extern int mkl_get_max_threads();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        public static extern void mkl_set_dynamic(ref int n);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        public static extern int mkl_get_dynamic();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        public static extern int mkl_set_threading_layer(ref int layer);

        #endregion

        static IntelVectorMathLibraryWrapper()
        {
            NativeLibraryPathHelper.AddLibraryPath();
        }

        public delegate int UnsafeVectorVectorOperation(int n,
            double* left, double* right, double* result);

        public delegate int UnsafeVectorOperation(int n,
            double* left, double* result);

        public static void CallUnsafe(double[] left, int leftStartIndex,
            double[] right, int rightStartIndex,
            double[] result, int resultStartIndex,
            int length, UnsafeVectorVectorOperation operation)
        {
            fixed (double* p_left = &left[leftStartIndex])
            {
                fixed (double* p_right = &right[rightStartIndex])
                {
                    fixed (double* p_result = &result[resultStartIndex])
                    {
                        operation(length, p_left, p_right, p_result);
                    }
                }
            }
        }

        public static void CallUnsafe(double[] left, int leftStartIndex,
            double[] result, int resultStartIndex,
            int length, UnsafeVectorOperation operation)
        {
            fixed (double* p_left = &left[leftStartIndex])
            {
                fixed (double* p_result = &result[resultStartIndex])
                {
                    operation(length, p_left, p_result);
                }
            }
        }

        public static void ConstantAddMultiply(double[] left, int leftStartIndex,
            double aScale, double aOffset,
            double[] result, int resultStartIndex,
            int length)
        {
            fixed (double* p_left = &left[leftStartIndex])
            {
                fixed (double* p_result = &result[resultStartIndex])
                {
                    vdLinearFrac(length, p_left, p_left, aScale, aOffset, 0.0, 1.0, p_result);
                }
            }
        }

        public static void Add(double[] left, int leftStartIndex,
            double[] right, int rightStartIndex,
            double[] result, int resultStartIndex,
            int length)
        {
            CallUnsafe(left, leftStartIndex, right, rightStartIndex, result, resultStartIndex, length, vdAdd);
        }

        public static void Subtract(double[] left, int leftStartIndex,
            double[] right, int rightStartIndex,
            double[] result, int resultStartIndex,
            int length)
        {
            CallUnsafe(left, leftStartIndex, right, rightStartIndex, result, resultStartIndex, length, vdSub);
        }

        public static void Multiply(double[] left, int leftStartIndex,
            double[] right, int rightStartIndex,
            double[] result, int resultStartIndex,
            int length)
        {
            CallUnsafe(left, leftStartIndex, right, rightStartIndex, result, resultStartIndex, length, vdMul);
        }

        public static void Divide(double[] left, int leftStartIndex,
            double[] right, int rightStartIndex,
            double[] result, int resultStartIndex,
            int length)
        {
            CallUnsafe(left, leftStartIndex, right, rightStartIndex, result, resultStartIndex, length, vdDiv);
        }

        public static void CumulativeNormal(double[] left, int leftStartIndex,
            double[] result, int resultStartIndex,
            int length)
        {
            CallUnsafe(left, leftStartIndex, result, resultStartIndex, length, vdCdfNorm);
        }

        public static void Exp(double[] left, int leftStartIndex,
            double[] result, int resultStartIndex,
            int length)
        {
            CallUnsafe(left, leftStartIndex, result, resultStartIndex, length, vdExp);
        }

        public static void Inverse(double[] left, int leftStartIndex,
            double[] result, int resultStartIndex,
            int length)
        {
            CallUnsafe(left, leftStartIndex, result, resultStartIndex, length, vdInv);
        }

        public static void InverseCumulativeNormal(double[] left, int leftStartIndex,
            double[] result, int resultStartIndex,
            int length)
        {
            CallUnsafe(left, leftStartIndex, result, resultStartIndex, length, vdCdfNormInv);
        }

        public static void InverseSquareRoot(double[] left, int leftStartIndex,
            double[] result, int resultStartIndex,
            int length)
        {
            CallUnsafe(left, leftStartIndex, result, resultStartIndex, length, vdInvSqrt);
        }

        public static void Log(double[] left, int leftStartIndex,
            double[] result, int resultStartIndex,
            int length)
        {
            CallUnsafe(left, leftStartIndex, result, resultStartIndex, length, vdLn);
        }

        public static void SquareRoot(double[] left, int leftStartIndex,
            double[] result, int resultStartIndex,
            int length)
        {
            CallUnsafe(left, leftStartIndex, result, resultStartIndex, length, vdSqrt);
        }

        public static void SortInPlace(double[] left, int leftStartIndex, int length)
        {
            char order = 'I'; // 'D'
            fixed (double* p_left = &left[leftStartIndex])
            {
                LAPACKE_dlasrt(order, length, p_left);
            }
        }

        public static void SetAccuracyMode(VMLAccuracy accuracy)
        {
            int intAccuracy = (int)accuracy;
            vmlSetMode(ref intAccuracy);
        }

        public static void SetSequential()
        {
            int mode = 1; // 0 = threaded; 1 = sequential 
            mkl_set_threading_layer(ref mode);
            int nThreads = 1;
            mkl_set_num_threads(ref nThreads);
        }
    }

    public enum CBLAS_ORDER
    {
        CblasRowMajor = 101,    /* row-major arrays */
        CblasColMajor = 102     /* column-major arrays */
    };

    public enum CBLAS_TRANSPOSE
    {
        CblasNoTrans = 111,     /* trans='N' */
        CblasTrans = 112,       /* trans='T' */
        CblasConjTrans = 113    /* trans='C' */
    };

    public enum CBLAS_UPLO
    {
        CblasUpper = 121,       /* uplo ='U' */
        CblasLower = 122        /* uplo ='L' */
    };

    public enum CBLAS_DIAG
    {
        CblasNonUnit = 131,     /* diag ='N' */
        CblasUnit = 132         /* diag ='U' */
    };

    public enum CBLAS_SIDE
    {
        CblasLeft = 141,        /* side ='L' */
        CblasRight = 142        /* side ='R' */
    };

    public enum VMLAccuracy
    {
        LowAccuracy = 1, HighAccuracy = 2, EnhancedPerformanceAccuracy = 3
    }

    class NativeLibraryPathHelper
    {
        public static void AddLibraryPath()
        {
            lock (locker)
            {
                if (_alreadyCalled) return;
                _alreadyCalled = true;
            }
            string path = Environment.GetEnvironmentVariable("PATH");
            string newPath = Path.Combine(AssemblyDirectory, Environment.Is64BitProcess ? "x64" : "x86");
            Environment.SetEnvironmentVariable("PATH",
                string.Join(";", newPath, path));
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        private static object locker = new object();
        private static bool _alreadyCalled = false;
    }
}
