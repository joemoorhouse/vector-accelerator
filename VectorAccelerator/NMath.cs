using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator.Properties;

namespace VectorAccelerator
{
    public enum Transpose { Tanspose, NoTranspose };

    public static class NMath
    {
        public static NArray Exp(NArray operand)
        {
            return ExecutionContext.Executor.UnaryElementWiseOperation(operand, UnaryElementWiseOperation.Exp) as NArray;
        }

        public static NArray Log(NArray operand)
        {
            return ExecutionContext.Executor.UnaryElementWiseOperation(operand, UnaryElementWiseOperation.Log) as NArray;
        }

        public static NArray CumulativeNormal(NArray operand)
        {
            return ExecutionContext.Executor.UnaryElementWiseOperation(operand, UnaryElementWiseOperation.CumulativeNormal) as NArray;
        }

        public static NArray InverseCumulativeNormal(NArray operand)
        {
            return ExecutionContext.Executor.UnaryElementWiseOperation(operand, UnaryElementWiseOperation.InverseCumulativeNormal) as NArray;
        }

        public static NArray Sqrt(NArray operand)
        {
            return ExecutionContext.Executor.UnaryElementWiseOperation(operand, UnaryElementWiseOperation.SquareRoot) as NArray;
        }

        public static NArray InvSqrt(NArray operand)
        {
            return ExecutionContext.Executor.UnaryElementWiseOperation(operand, UnaryElementWiseOperation.InverseSquareRoot) as NArray;
        }

        public static void MatrixMultiply(NArray a, NArray b, NArray c, 
            Transpose aTranspose = Transpose.NoTranspose, Transpose bTranspose = Transpose.NoTranspose)
        {
            ExecutionContext.Executor.MatrixMultiply(a, b, c);
        }

        public static double Dot(NArray a, NArray b)
        {
            return ExecutionContext.Executor.DotProduct(a, b);
        }

        public static NArray CholeskyDecomposition(NArray a)
        {
            var result = a.Clone(MatrixRegion.LowerTriangle);
            ExecutionContext.Executor.CholeskyDecomposition(result);
            return result;
        }

        public static NArray Diagonal(NArray a)
        {
            return new NArray(a.Storage.Diagonal(a.Length, a.Length));
        }

        public static NArray Diagonal(NArray a, int rowCount, int columnCount)
        {
            return new NArray(a.Storage.Diagonal(rowCount, columnCount));
        }

        public static void EigenvalueDecomposition(NArray a, NArray eigenvectors, NArray eigenvalues)
        {
            ExecutionContext.Executor.EigenvalueDecomposition(a, eigenvectors, eigenvalues);
        }

        public static void EigenvalueDecomposition(NArray a, out NArray eigenvectors, out NArray eigenvalues)
        {
            eigenvectors = NArrayFactory.CreateLike(a, a.RowCount, a.ColumnCount);
            eigenvalues = NArrayFactory.CreateLike(a, a.RowCount);
            EigenvalueDecomposition(a, eigenvectors, eigenvalues);
        }

        public static NArray<T> CreateNArray<T>(NArrayStorage<T> storage)
        {
            if (typeof(T) == typeof(double)) return new NArray(storage as NArrayStorage<double>) as NArray<T>;
            //else if (typeof(T) == typeof(int)) return new NArrayInt(storage as NArrayStorage<int>) as NArray<T>;
            else return null;
        }

        public static double Correlation(NArray a, NArray b)
        {
            var aSum = a.Sum();
            var bSum = b.Sum();
            var n = a.Length;

            return (NMath.Dot(a, b) * n - aSum * bSum)
                / Math.Sqrt((NMath.Dot(a, a) * n - aSum * aSum) * (NMath.Dot(b, b) * n - bSum * bSum));
        }

        public static IEnumerable<double> Percentiles(NArray a, double[] percentiles)
        {
            var clone = a.Clone();
            SortInPlace(clone);
            // the ith element is percentile p = 100 * (i + 0.5) / n
            // i = n * p / 100 - 0.5
            var fractionalIndices = percentiles.Select(p => clone.Length * p / 100.0 - 0.5);
            foreach (var fractionalIndex in fractionalIndices)
            {
                var lower = (int)fractionalIndex;
                var weightUpper = fractionalIndex - lower;
                yield return clone.GetValue(lower) * (1 - weightUpper) + weightUpper * clone.GetValue(lower + 1);
            }
        }

        //public static IEnumerable<double> Percentiles(NArray a, params double[] percentiles)
        //{
        //    return Percentiles(a, percentiles.ToArray());
        //}

        public static void SortInPlace(NArray a)
        {
            ExecutionContext.Executor.SortInPlace(a);
        }
    }

    public static class NExtensions
    {
        /// <summary>
        /// New extension
        /// </summary>
        /// <param name="cubes"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static IEnumerable<NArray> Select(this IList<NArray> cubes, Func<NArray, int, NArray> func)
        {
            return cubes;
        }
        
        public static IEnumerable<NArray> SelectN(this IList<NArray> cubes, Func<NArray, int, NArray> func)
        {
            return cubes;
        }

        public static NArray Sum(this IEnumerable<NArray> cubes)
        {
            return null;
        }

        //public static void While(Func<NArray, NArray predicate, Action action)
        //{
        //
        //}
    }
}
