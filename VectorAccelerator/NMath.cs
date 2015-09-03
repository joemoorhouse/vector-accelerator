using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace VectorAccelerator
{
    public static class NMath
    {
        public static NArray Exp(NArray operand)
        {
            return ExecutionContext.Executor.ElementWiseExp(operand);
        }

        public static NArray Log(NArray operand)
        {
            return ExecutionContext.Executor.ElementWiseLog(operand);
        }

        public static NArray CumulativeNormal(NArray operand)
        {
            return ExecutionContext.Executor.ElementWiseCumulativeNormal(operand);
        }

        public static NArray InverseCumulativeNormal(NArray operand)
        {
            return ExecutionContext.Executor.ElementWiseInverseCumulativeNormal(operand);
        }

        public static NArray Sqrt(NArray operand)
        {
            return ExecutionContext.Executor.ElementWiseSquareRoot(operand);
        }

        public static NArray InvSqrt(NArray operand)
        {
            return ExecutionContext.Executor.ElementWiseInverseSquareRoot(operand);
        }
        //public static While(NArray predicta)
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
