using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using VectorAccelerator.NArrayStorage;

namespace VectorAccelerator.LinearAlgebraProviders
{
    public delegate void VectorVectorOperation(double[] a, int aStartIndex,
        double[] b, int bStartIndex,
        double[] y, int yStartIndex,
        int length);

    public delegate void VectorScalarOperation(double[] a, int aStartIndex,
        double b,
        double[] y, int yStartIndex,
        int length);

    public class IntelMKLLinearAlgebraProvider : IImmediateLinearAlgebraProvider
    {   
        public NAray Add(NAray a, NAray b)
        {
            return VectorVectorOperation(a, b, IntelMathKernalLibrary.Add);
        }

        public NAray Subtract(NAray a, NAray b)
        {
            return VectorVectorOperation(a, b, IntelMathKernalLibrary.Subtract);
        }

        public NAray Multiply(NAray a, NAray b)
        {
            return VectorVectorOperation(a, b, IntelMathKernalLibrary.Multiply);
        }

        public NAray Divide(NAray a, NAray b)
        {
            return VectorVectorOperation(a, b, IntelMathKernalLibrary.Divide);
        }

        public NAray ScaleOffset(NAray a, double scale, double offset)
        {
            var newNArray = NAray.CreateLike(a);
            IntelMathKernalLibrary.ConstantAddMultiply(GetArray(a), 0, scale, offset, GetArray(newNArray), 0, newNArray.Length);
            return newNArray;
        }

        public NAray VectorVectorOperation(NAray a, NAray b, VectorVectorOperation operation)
        {
            var newNArray = NAray.CreateLike(a, b);
            operation(GetArray(a), 0, GetArray(b), 0, GetArray(newNArray), 0, newNArray.Length);
            return newNArray;
        }

        public NAray VectorScalarOperation(NAray a, double b, VectorScalarOperation operation)
        {
            var newNArray = NAray.CreateLike(a);
            operation(GetArray(a), 0, b, GetArray(newNArray), 0, newNArray.Length);
            return newNArray;
        }

        private double[] GetArray(NAray vector)
        {
            var managedStorage = vector.Storage as ManagedStorage<double>;
            if (managedStorage == null)
            {
                throw new ArgumentException("storage not managed or mismatching.");
            }
            return managedStorage.Array;
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
        internal static extern int vdExp(int n,
            double* a, double* y);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int vdLog(int n,
            double* a, double* y);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        internal static extern int cblas_daxpy(int n,
            double a, double* x, int inc_x, double[] y, int inc_y);

        public delegate int UnsafeVectorOperation(int n,
            double* a, double* b, double* y);

        public static void CallUnsafe(double[] a, int aStartIndex,
            double[] b, int bStartIndex,
            double[] y, int yStartIndex,
            int length, UnsafeVectorOperation operation)
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
    }
}
