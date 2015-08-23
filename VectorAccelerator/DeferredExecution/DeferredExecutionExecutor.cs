using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace VectorAccelerator.DeferredExecution
{   
    class DeferredExecutionExecutor : IExecutor
    {
        ExpressionBuilder _builder = new ExpressionBuilder();

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

        public void Assign(NAray operand1, NAray operand2)
        {
            _builder.Assign(operand1, operand2);
        }

        #region Binary Operations

        public NAray ElementWiseAdd(NAray operand1, NAray operand2)
        {
            return _builder.BinaryExpression(operand1, operand2, Expression.Add);
        }

        public NAray ElementWiseAdd(NAray operand1, double operand2)
        {
            return _builder.BinaryExpression(operand1, new NAray(operand2), Expression.Add);
        }

        public NAray ElementWiseAdd(double operand1, NAray operand2)
        {
            return _builder.BinaryExpression(new NAray(operand1), operand2, Expression.Add);
        }

        public NAray ElementWiseSubtract(NAray operand1, NAray operand2)
        {
            return _builder.BinaryExpression(operand1, operand2, Expression.Subtract);
        }

        public NAray ElementWiseSubtract(NAray operand1, double operand2)
        {
            return _builder.BinaryExpression(operand1, new NAray(operand2), Expression.Subtract);
        }

        public NAray ElementWiseSubtract(double operand1, NAray operand2)
        {
            return _builder.BinaryExpression(new NAray(operand1), operand2, Expression.Subtract);
        }

        public NAray ElementWiseMultiply(NAray operand1, NAray operand2)
        {
            return _builder.BinaryExpression(operand1, operand2, Expression.Multiply);
        }

        public NAray ElementWiseMultiply(NAray operand1, double operand2)
        {
            return _builder.BinaryExpression(operand1, new NAray(operand2), Expression.Multiply);
        }

        public NAray ElementWiseMultiply(double operand1, NAray operand2)
        {
            return _builder.BinaryExpression(new NAray(operand1), operand2, Expression.Multiply);
        }

        public NAray ElementWiseDivide(NAray operand1, NAray operand2)
        {
            return _builder.BinaryExpression(operand1, operand2, Expression.Divide);
        }

        public NAray ElementWiseDivide(NAray operand1, double operand2)
        {
            return _builder.BinaryExpression(operand1, new NAray(operand2), Expression.Divide);
        }

        public NAray ElementWiseDivide(double operand1, NAray operand2)
        {
            return _builder.BinaryExpression(new NAray(operand1), operand2, Expression.Divide);
        }

        #endregion

        #region Unary Operations

        public NAray ElementWiseExp(NAray operand)
        {
            return _builder.MethodCallExpression(operand, exponential);
        }

        public NAray ElementWiseLog(NAray operand)
        {
            return _builder.MethodCallExpression(operand, log);
        }

        public NAray ElementWiseNegate(NAray operand)
        {
            return _builder.UnaryExpression(operand, Expression.Negate);
        }

        #endregion
    }
}
