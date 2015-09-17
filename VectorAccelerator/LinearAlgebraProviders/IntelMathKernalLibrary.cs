using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using VectorAccelerator.NArrayStorage;

namespace VectorAccelerator.LinearAlgebraProviders
{
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
    
    public delegate void VectorVectorOperation(double[] a, int aStartIndex,
        double[] b, int bStartIndex,
        double[] y, int yStartIndex,
        int length);

    public delegate void VectorOperation(double[] a, int aStartIndex,
        double[] y, int yStartIndex,
        int length);

    public class IntelMKLLinearAlgebraProvider : ILinearAlgebraProvider
    {   
        public void Add(NArray a, NArray b, NArray result)
        {
            VectorVectorOperation(a, b, result, IntelMathKernalLibrary.Add);
        }

        public void Subtract(NArray a, NArray b, NArray result)
        {
            VectorVectorOperation(a, b, result, IntelMathKernalLibrary.Subtract);
        }

        public void Multiply(NArray a, NArray b, NArray result)
        {
            VectorVectorOperation(a, b, result, IntelMathKernalLibrary.Multiply);
        }

        public void Divide(NArray a, NArray b, NArray result)
        {
            VectorVectorOperation(a, b, result, IntelMathKernalLibrary.Divide);
        }

        public void Inverse(NArray a, NArray result)
        {
            VectorOperation(a, result, IntelMathKernalLibrary.Inverse);
        }

        public void Exp(NArray a, NArray result)
        {
            VectorOperation(a, result, IntelMathKernalLibrary.Exp);
        }

        public void Log(NArray a, NArray result)
        {
            VectorOperation(a, result, IntelMathKernalLibrary.Log);
        }

        public void SquareRoot(NArray a, NArray result)
        {
            VectorOperation(a, result, IntelMathKernalLibrary.SquareRoot);
        }

        public void InverseSquareRoot(NArray a, NArray result)
        {
            VectorOperation(a, result, IntelMathKernalLibrary.InverseSquareRoot);
        }

        public void CumulativeNormal(NArray a, NArray result)
        {
            VectorOperation(a, result, IntelMathKernalLibrary.CumulativeNormal);
        }

        public void InverseCumulativeNormal(NArray a, NArray result)
        {
            VectorOperation(a, result, IntelMathKernalLibrary.InverseCumulativeNormal);
        }

        public void ScaleOffset(NArray a, double scale, double offset, NArray result)
        {
            double[] aArray, resultArray;
            int aStart, resultStart;
            GetArray(a, out aArray, out aStart);
            GetArray(result, out resultArray, out resultStart);
            IntelMathKernalLibrary.ConstantAddMultiply(aArray, aStart, scale, offset, resultArray, resultStart, result.Length);
        }

        public IRandomNumberGenerator CreateRandomNumberGenerator(RandomNumberGeneratorType type, int seed)
        {
            return new IntelMKLRandomNumberGenerator(type, seed);
        }

        public NArray CreateLike(NArray a)
        {
            return NArray.CreateLike(a);
        }

        public void VectorVectorOperation(NArray a, NArray b, NArray result, VectorVectorOperation operation)
        {
            double[] aArray, bArray, resultArray;
            int aStart, bStart, resultStart;
            GetArray(a, out aArray, out aStart);
            GetArray(b, out bArray, out bStart);
            GetArray(result, out resultArray, out resultStart);
            operation(aArray, aStart, bArray, bStart, resultArray, resultStart, result.Length);
        }

        public void VectorOperation(NArray a, NArray result, VectorOperation operation)
        {
            double[] aArray, resultArray;
            int aStart, resultStart;
            GetArray(a, out aArray, out aStart);
            GetArray(result, out resultArray, out resultStart);
            operation(aArray, aStart, resultArray, resultStart, result.Length);
        }

        private void GetArray(NArray vector, out double[] array, out int startIndex)
        {
            var managedStorage = vector.Storage as ManagedStorage<double>;
            if (managedStorage == null)
            {
                throw new ArgumentException("storage not managed or mismatching.");
            }
            array = managedStorage.Array;
            startIndex = managedStorage.ArrayStart;
        }
    }
    
    public unsafe static class IntelMathKernalLibrary
    {
        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern double cblas_ddot(
            int n, double* x, int inc_x, double* y, int inc_y);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int cblas_dgemm(
            int Order, int TransA, int TransB, int M, int N, int K,
            double alpha, [In] double[,] A, int lda, [In] double[,] B, int ldb,
            double beta, [In, Out] double[,] C, int ldc);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdLinearFrac(int n,
            double* a, double* b, double scale_a, double shift_a, double scale_b, double shift_b, double* y);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdAdd(int n,
            double* a, double* b, double* y);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdSub(int n,
            double* a, double* b, double* y);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdMul(int n,
            double* a, double* b, double* y);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdDiv(int n,
            double* a, double* b, double* y);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdInv(int n,
            double* a, double* y);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdExp(int n,
            double* a, double* y);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdLn(int n,
            double* a, double* y);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdSqrt(int n,
            double* a, double* y);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdInvSqrt(int n,
            double* a, double* y);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdCdfNorm(int n,
            double* a, double* y);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdCdfNormInv(int n,
            double* a, double* y);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int cblas_daxpy(int n,
            double a, double* x, int inc_x, double[] y, int inc_y);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        public static extern int vmlSetMode(ref int mode);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        public static extern void mkl_set_num_threads(ref int n);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        public static extern int mkl_get_max_threads();

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        public static extern void mkl_set_dynamic(ref int n);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        public static extern int mkl_get_dynamic();

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        public static extern int mkl_set_threading_layer(ref int layer);

        static IntelMathKernalLibrary()
        {
            // if necessary, can set threading mode here. Most commonly threading would be done
            // at higher level than MKL call (i.e. with .NET threads), in which case we want sequential mode.
            //int mode = 1; // 0 = threaded; 1 = sequential 
            //mkl_set_threading_layer(ref mode);
            //int nThreads = 2;
            //mkl_set_num_threads(ref nThreads);
        }

        public delegate int UnsafeVectorVectorOperation(int n,
            double* a, double* b, double* y);

        public delegate int UnsafeVectorOperation(int n,
            double* a, double* y);

        public static void CallUnsafe(double[] a, int aStartIndex,
            double[] b, int bStartIndex,
            double[] y, int yStartIndex,
            int length, UnsafeVectorVectorOperation operation)
        {
            fixed (double* p_a = &a[aStartIndex])
            {
                fixed (double* p_b = &b[bStartIndex])
                {
                    fixed (double* p_y = &y[yStartIndex])
                    {
                        operation(length, p_a, p_b, p_y);
                    }
                }
            }
        }

        public static void CallUnsafe(double[] a, int aStartIndex,
            double[] y, int yStartIndex,
            int length, UnsafeVectorOperation operation)
        {
            fixed (double* p_a = &a[aStartIndex])
            {
                fixed (double* p_y = &y[yStartIndex])
                {
                    operation(length, p_a, p_y);
                }
            }
        }

        public static void ConstantAddMultiply(double[] a, int aStartIndex,
            double aScale, double aOffset,
            double[] y, int yStartIndex,
            int length)
        {
            fixed (double* p_a = &a[aStartIndex])
            {
                fixed (double* p_y = &y[yStartIndex])
                {
                    vdLinearFrac(length, p_a, p_a, aScale, aOffset, 0, 1, p_y);
                }
            }
        }

        public static void Add(double[] a, int aStartIndex,
            double[] b, int bStartIndex,
            double[] y, int yStartIndex,
            int length)
        {
            CallUnsafe(a, aStartIndex, b, bStartIndex, y, yStartIndex, length, vdAdd);
        }

        public static void Subtract(double[] a, int aStartIndex,
            double[] b, int bStartIndex,
            double[] y, int yStartIndex,
            int length)
        {
            CallUnsafe(a, aStartIndex, b, bStartIndex, y, yStartIndex, length, vdSub);
        }

        public static void Multiply(double[] a, int aStartIndex,
            double[] b, int bStartIndex,
            double[] y, int yStartIndex,
            int length)
        {
            CallUnsafe(a, aStartIndex, b, bStartIndex, y, yStartIndex, length, vdMul);
        }

        public static void Divide(double[] a, int aStartIndex,
            double[] b, int bStartIndex,
            double[] y, int yStartIndex,
            int length)
        {
            CallUnsafe(a, aStartIndex, b, bStartIndex, y, yStartIndex, length, vdDiv);
        }

        public static void Inverse(double[] a, int aStartIndex,
            double[] y, int yStartIndex,
            int length)
        {
            CallUnsafe(a, aStartIndex, y, yStartIndex, length, vdInv);
        }

        public static void Exp(double[] a, int aStartIndex,
            double[] y, int yStartIndex,
            int length)
        {
            CallUnsafe(a, aStartIndex, y, yStartIndex, length, vdExp);
        }

        public static void Log(double[] a, int aStartIndex,
            double[] y, int yStartIndex,
            int length)
        {
            CallUnsafe(a, aStartIndex, y, yStartIndex, length, vdLn);
        }

        public static void SquareRoot(double[] a, int aStartIndex,
            double[] y, int yStartIndex,
            int length)
        {
            CallUnsafe(a, aStartIndex, y, yStartIndex, length, vdSqrt);
        }

        public static void InverseSquareRoot(double[] a, int aStartIndex,
            double[] y, int yStartIndex,
            int length)
        {
            CallUnsafe(a, aStartIndex, y, yStartIndex, length, vdInvSqrt);
        }

        public static void CumulativeNormal(double[] a, int aStartIndex,
            double[] y, int yStartIndex,
            int length)
        {
            CallUnsafe(a, aStartIndex, y, yStartIndex, length, vdCdfNorm);
        }

        public static void InverseCumulativeNormal(double[] a, int aStartIndex,
            double[] y, int yStartIndex,
            int length)
        {
            CallUnsafe(a, aStartIndex, y, yStartIndex, length, vdCdfNormInv);
        }

        public static double Dot(double[] a, int aStartIndex,
            double[] b, int bStartIndex,
            int length)
        {
            fixed (double* p_a = &a[aStartIndex])
            {
                fixed (double* p_b = &b[bStartIndex])
                {
                    return cblas_ddot(length, p_a, 1, p_b, 1);
                }
            }
        }

        /// <summary>
        /// C = A * B (* denotes matrix multiplication)
        /// </summary>
        public static void MatrixMultiply(double[,] A, double[,] B, double[,] C, bool shouldTransposeA = false, bool shouldTransposeB = false)
        {
            int rowsA = shouldTransposeA ? A.GetLength(1) : A.GetLength(0);
            int colsA = shouldTransposeA ? A.GetLength(0) : A.GetLength(1);
            int rowsB = shouldTransposeB ? B.GetLength(1) : B.GetLength(0);
            int colsB = shouldTransposeB ? B.GetLength(0) : B.GetLength(1);
            int rowsC = C.GetLength(0);
            int colsC = C.GetLength(1);
            if (colsA != rowsB) throw new Exception("A and B are not compatible sizes.");
            if ((rowsC != rowsA) || (colsC != colsB)) throw new Exception("C is incorrectly sized for output.");
            int transposeA = shouldTransposeA ? (int)CBLAS_TRANSPOSE.CblasTrans : (int)CBLAS_TRANSPOSE.CblasNoTrans;
            int transposeB = shouldTransposeB ? (int)CBLAS_TRANSPOSE.CblasTrans : (int)CBLAS_TRANSPOSE.CblasNoTrans;
            int status = cblas_dgemm((int)CBLAS_ORDER.CblasRowMajor, transposeA, transposeB, rowsA, colsB,
                colsA, 1.0, A, A.GetLength(1), B, B.GetLength(1), 0.0, C, C.GetLength(1));
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
}
