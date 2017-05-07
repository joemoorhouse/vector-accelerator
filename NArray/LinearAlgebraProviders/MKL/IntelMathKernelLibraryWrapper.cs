using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NArray.LinearAlgebraProviders
{
    public unsafe static class IntelMathKernelLibraryWrapper
    {
        [DllImport(IntelMathKernelLibrary.DllName, CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
            internal static extern int LAPACKE_dgels(int matrix_layout, char trans, int m, int n, int nrhs,
            double[] a, int lda, double[] b, int ldb);

        public static double[] PseudoInverse(double[] a, int rows, int columns)
        {
            var result = new double[a.Length];
            // assume row-major
            int lda = columns;
            int ldb = columns;
            var b = Diagonal(columns);
            var status = LAPACKE_dgels((int)LAPACK_ORDER.LAPACKRowMajor, 'N', rows, columns, columns, a, lda, b, ldb);
            return b;
        }

        public static double[] Diagonal(int rows)
        {
            var result = new double[rows];
            for (int i = 0; i < rows; ++i) result[i] = 1.0;
            return result;
        }
    }

    public enum MatrixOrder { ColumnMajor, RowMajor };

    public enum CBLAS_ORDER
    {
        CblasRowMajor = 101,    /* row-major arrays */
        CblasColMajor = 102     /* column-major arrays */
    };

    public enum LAPACK_ORDER
    {
        LAPACKRowMajor = 101,    /* row-major arrays */
        LAPACKColMajor = 102     /* column-major arrays */
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
