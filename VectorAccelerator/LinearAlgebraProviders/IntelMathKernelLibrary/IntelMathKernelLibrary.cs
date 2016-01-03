using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using VectorAccelerator.NArrayStorage;
using VectorAccelerator.Distributions;

namespace VectorAccelerator.LinearAlgebraProviders
{    
    public unsafe static class IntelMathKernelLibrary
    {
        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern double cblas_ddot(
            int n, double* x, int inc_x, double* y, int inc_y);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int cblas_dgemm(
            int order, int transA, int transB, int m, int n, int k,
            double alpha, double* A, int lda, double* B, int ldb,
            double beta, double* C, int ldc);

        // Cholesky decomposition
        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern void DPOTRF(ref char uplo, ref int n, double* A, ref int lda, ref int info);

        // Eigenvalue decomposition
        [DllImport("mkl_rt.dll", EntryPoint = "DSYEVR", CallingConvention = CallingConvention.Cdecl)]
        private static extern void DSYEVR(ref char jobz, ref char range, ref char uplo, ref int n, 
            double* A, ref int lda, ref double vl, ref double vu, ref int il, ref int iu, 
            ref double abstol, ref int m, double* w, double* z, ref int ldz, 
            int[] isuppz, double[] work, ref int lwork, int[] iwork, ref int liwork, ref int info);

        [DllImport("mkl_rt.dll", EntryPoint = "DSYEVR", CallingConvention = CallingConvention.Cdecl)]
        private static extern void DSYEVR2(ref char jobz, ref char range, ref char uplo, ref int n, 
            double[] A, ref int lda, ref double vl, ref double vu, ref int il, ref int iu, 
            ref double abstol, ref int m, double[] w, double[] z, ref int ldz, 
            int[] isuppz, double[] work, ref int lwork, int[] iwork, ref int liwork, ref int info);

        [DllImport("mkl_rt.dll", EntryPoint = "LAPACKE_dlasrt", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int LAPACKE_dlasrt(char id, int n, double* d);

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

        static IntelMathKernelLibrary()
        {
            NativeLibraryHelper.AddLibraryPath();
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

        public static double Dot(ManagedStorage<double> a, ManagedStorage<double> b)
        {
            fixed (double* p_a = &a.Array[a.ArrayStart])
            {
                fixed (double* p_b = &b.Array[b.ArrayStart])
                {
                    return cblas_ddot(a.Length, p_a, 1, p_b, 1);
                }
            }
        }

        /// <summary>
        /// C = A * B (where '*' denotes matrix multiplication)
        /// </summary>
        public static void MatrixMultiply(ManagedStorage<double> a, ManagedStorage<double> b, ManagedStorage<double> c, 
            bool aShouldTranspose = false, bool bShouldTranspose = false)
        {
            int aRows = aShouldTranspose ? a.ColumnCount : a.RowCount; int aCols = aShouldTranspose ? a.RowCount : a.ColumnCount;
            int bRows = bShouldTranspose ? b.ColumnCount : b.RowCount; int bCols = bShouldTranspose ? b.RowCount : b.ColumnCount;
            int cRows = c.RowCount;
            int cCols = c.ColumnCount;
            if (aCols != bRows) throw new Exception("A and B are not compatible sizes.");
            if ((cRows != aRows) || (cCols != bCols)) throw new Exception("C is incorrectly sized for output.");
            int transposeA = aShouldTranspose ? (int)CBLAS_TRANSPOSE.CblasTrans : (int)CBLAS_TRANSPOSE.CblasNoTrans;
            int transposeB = bShouldTranspose ? (int)CBLAS_TRANSPOSE.CblasTrans : (int)CBLAS_TRANSPOSE.CblasNoTrans;
            fixed (double* p_a = &a.Array[a.ArrayStart])
            {
                fixed (double* p_b = &b.Array[b.ArrayStart])
                {
                    fixed (double* p_c = &c.Array[c.ArrayStart])
                    {
                        int status = cblas_dgemm((int)CBLAS_ORDER.CblasColMajor, transposeA, transposeB, aRows, bCols,
                            aCols, 1.0, p_a, a.RowCount, p_b, b.RowCount, 0.0, p_c, c.RowCount);
                    }
                }
            }
        }

        /// <summary>
        /// Performs Cholesky decomposition of a matrix in place.
        /// </summary>
        /// <param name="matrix">The upper triangular part contains symmetric matrix on entry.
        /// Lower triangular part contains decomposition on exit.</param>
        /// <param name="positiveSemiDefinite">Whether matrix was positive semi-definite.</param>
        public static void CholeskyDecomposition(ManagedStorage<double> a,
            out bool positiveSemiDefinite)
        {
            if (a.RowCount != a.ColumnCount) throw new ArgumentException("matrix must be square.");
            int lda = a.ColumnCount;
            int info = -1;
            int n = a.ColumnCount; //subMatrixDimension; // to only transform sub-matrix 
            char uplo = 'L'; 
            fixed (double* p_a = &a.Array[a.ArrayStart])
            {
                DPOTRF(ref uplo, ref n, p_a, ref lda, ref info);
            }
            positiveSemiDefinite = true;
            if (info != 0) positiveSemiDefinite = false;
        }

        /// <summary>
        /// Performs Eigenvalue decomposition.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="eigenvectors"></param>
        /// <param name="eigenvalues"></param>
        public static void EigenvalueDecomposition(ManagedStorage<double> a, ManagedStorage<double> eigenvectors,
            ManagedStorage<double> eigenvalues)
        {
            int m = 0, info = 0;
            int n = a.RowCount; 
            var aClone = a.Clone();

            int[] isuppz = new int[2 * n];

            char jobz = 'V'; char range = 'A'; char uplo = 'U';
            double[] work = new double[1];
            int lwork = -1, liwork = -1;
            int[] iwork = new int[1];
            int lda = n; int ldz = n; int il, iu;
            double vl, vu, abstol; vl = vu = abstol = 0;
            il = iu = 0;

            fixed (double* p_a = &aClone.Array[aClone.ArrayStart])
            {
                fixed (double* p_eigenvectors = &eigenvectors.Array[eigenvectors.ArrayStart])
                {
                    fixed (double* p_eigenvalues = &eigenvalues.Array[eigenvalues.ArrayStart])
                    {
                        DSYEVR(ref jobz, ref range, ref uplo, ref n, p_a, ref lda, ref vl, ref vu, ref il, ref iu, ref abstol, ref m, 
                            p_eigenvalues, p_eigenvectors, ref ldz, isuppz, work, ref lwork, iwork, ref liwork, ref info);

                        if (info != 0)
                        {
                            throw new Exception("Eigenvalue decomposition error.");
                        }
                        lwork = (int)work[0];
                        work = new double[lwork];
                        liwork = (int)iwork[0];
                        iwork = new int[liwork];
                        DSYEVR(ref jobz, ref range, ref uplo, ref n, p_a, ref lda, ref vl, ref vu, ref il, ref iu, ref abstol, ref m, 
                            p_eigenvalues, p_eigenvectors, ref ldz, isuppz, work, ref lwork, iwork, ref liwork, ref info);
                    }
                }
            }
        }

        public static void SortInPlace(ManagedStorage<double> a)
        {
            char order = 'I'; // 'D'
            fixed (double* p_a = &a.Array[a.ArrayStart])
            {
                LAPACKE_dlasrt(order, a.Length, p_a);
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
}
