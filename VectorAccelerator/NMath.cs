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
        public static NAray Exp(NAray operand)
        {
            return ExecutionContext.Executor.ElementWiseExp(operand);
        }

        public static NAray Log(NAray operand)
        {
            return ExecutionContext.Executor.ElementWiseLog(operand);
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
        public static IEnumerable<NAray> Select(this IList<NAray> cubes, Func<NAray, int, NAray> func)
        {
            return cubes;
        }
        
        public static IEnumerable<NAray> SelectN(this IList<NAray> cubes, Func<NAray, int, NAray> func)
        {
            return cubes;
        }

        public static NAray Sum(this IEnumerable<NAray> cubes)
        {
            return null;
        }

        //public static void While(Func<NArray, NArray predicate, Action action)
        //{
        //
        //}
    }
}
