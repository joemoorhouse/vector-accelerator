using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace VectorAccelerator.DeferredExecution
{   
    class ExpressionBuildingExecutor : IExecutor
    {
        ExpressionsBuilder _builder = new ExpressionsBuilder();

        static MethodInfo exponential = typeof(Math).GetMethod("Exp");
        static MethodInfo log = typeof(Math).GetMethod("Log", new Type[] { typeof(double), typeof(double) });
        static MethodInfo sin = typeof(Math).GetMethod("Sin");
        static MethodInfo cos = typeof(Math).GetMethod("Cos");


        internal void EndDeferredExecution()
        {
            var check = _builder.DebugViewExpressions();
            var block = _builder.BuildBlock();
            Console.WriteLine(check);
            //var visitor = new CUDACodeGenerator();
            //visitor.Visit(block);
            //visitor.GenerateCode(block);
            //var code = visitor.GetCode();
        }

        public void Assign(NArray operand1, NArray operand2)
        {
            _builder.Assign(operand1, operand2);
        }

        #region Binary Operations

        public NArray ElementWiseAdd(NArray operand1, NArray operand2)
        {
            return _builder.BinaryExpression(operand1, operand2, Expression.Add);
        }

        public NArray ElementWiseAdd(NArray operand1, double operand2)
        {
            return _builder.BinaryExpression(operand1, new NArray(operand2), Expression.Add);
        }

        public NArray ElementWiseAdd(double operand1, NArray operand2)
        {
            return _builder.BinaryExpression(new NArray(operand1), operand2, Expression.Add);
        }

        public NArray ElementWiseSubtract(NArray operand1, NArray operand2)
        {
            return _builder.BinaryExpression(operand1, operand2, Expression.Subtract);
        }

        public NArray ElementWiseSubtract(NArray operand1, double operand2)
        {
            return _builder.BinaryExpression(operand1, new NArray(operand2), Expression.Subtract);
        }

        public NArray ElementWiseSubtract(double operand1, NArray operand2)
        {
            return _builder.BinaryExpression(new NArray(operand1), operand2, Expression.Subtract);
        }

        public NArray ElementWiseMultiply(NArray operand1, NArray operand2)
        {
            return _builder.BinaryExpression(operand1, operand2, Expression.Multiply);
        }

        public NArray ElementWiseMultiply(NArray operand1, double operand2)
        {
            return _builder.BinaryExpression(operand1, new NArray(operand2), Expression.Multiply);
        }

        public NArray ElementWiseMultiply(double operand1, NArray operand2)
        {
            return _builder.BinaryExpression(new NArray(operand1), operand2, Expression.Multiply);
        }

        public NArray ElementWiseDivide(NArray operand1, NArray operand2)
        {
            return _builder.BinaryExpression(operand1, operand2, Expression.Divide);
        }

        public NArray ElementWiseDivide(NArray operand1, double operand2)
        {
            return _builder.BinaryExpression(operand1, new NArray(operand2), Expression.Divide);
        }

        public NArray ElementWiseDivide(double operand1, NArray operand2)
        {
            return _builder.BinaryExpression(new NArray(operand1), operand2, Expression.Divide);
        }

        #endregion

        #region Unary Operations

        public NArray ElementWiseNegate(NArray operand)
        {
            return _builder.UnaryExpression(operand, Expression.Negate);
        }

        public NArray ElementWiseExp(NArray operand)
        {
            return _builder.MethodCallExpression(operand, exponential);
        }

        public NArray ElementWiseLog(NArray operand)
        {
            return _builder.MethodCallExpression(operand, log);
        }

        public NArray ElementWiseSquareRoot(NArray operand)
        {
            return _builder.MethodCallExpression(operand, log);
        }

        public NArray ElementWiseInverseSquareRoot(NArray operand)
        {
            return _builder.MethodCallExpression(operand, log);
        }

        public NArray ElementWiseCumulativeNormal(NArray operand)
        {
            return _builder.MethodCallExpression(operand, log);
        }

        public NArray ElementWiseInverseCumulativeNormal(NArray operand)
        {
            return _builder.MethodCallExpression(operand, log);
        }

        #endregion
    }
}
