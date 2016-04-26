using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using VectorAccelerator;
using VectorAccelerator.Distributions;
using VectorAccelerator.LinearAlgebraProviders;
using VectorAccelerator.DeferredExecution;
using VectorAccelerator.DeferredExecution.Expressions;
namespace VectorAccelerator.DeferredExecution
{    
    class AlgorithmicDifferentiator
    {
        class Function
        {
            public VectorParameterExpression Parameter;
            public Expression Expression;

            public override string ToString()
            {
                return string.Format("{0} = {1}", Parameter.ToString(), Expression.ToString());
            }
        }
        
        /// <summary>
        /// Performs reverse-mode (adjoint) automatic differentiation 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="dependentVariable"></param>
        /// <param name="independentVariables"></param>
        public static void Differentiate(BlockExpressionBuilder builder, 
            out IList<Expression> derivativeExpressions,
            VectorParameterExpression dependentVariable,
            params VectorParameterExpression[] independentVariables)
        {
            builder.UpdateLocalsNumbering();
            var block = builder.ToBlock();
            // change the numbering of variables; arguments first, then locals      
            List<int>[] jLookup;
            Function[] f;

            // we want a list of functions which can be unary or binary (in principle high order if it makes sense) and a look-up
            // each function is associated with a parameter, which is either an argument or a local
            // for each function index we return a list of the indices of the functions that reference the parameter
            GetFunctions(block, out f, out jLookup);

            int N = dependentVariable.Index;

            // the list of operations needed to calculate the derivatives (*all* derivatives)
            derivativeExpressions = new List<Expression>(); 
            var dxNdx = new Expression[N + 1];
            dxNdx[N] = new ConstantExpression<double>(1.0); 
            for (int i = N - 1; i >= 0; i--)
            {
                // dxN / dxi
                // need to find all operation indices j such that p(j) contains i
                Expression total = new ConstantExpression<double>(0);
                var xi = f[i].Parameter;
                //need sum of dfj/dxi dXN/dxj
                foreach (var j in jLookup[i])
                {
                    var fj = f[j];
                    var dfjdxi = Differentiate(fj, xi); // dfj/dxi 
                    var dXNdxj = dxNdx[j]; // dXN/dxj
                    var product = builder.AddProductExpression(dfjdxi, dXNdxj);
                    total = builder.AddAdditionExpression(total, product);
                }
                dxNdx[i] = total;
                if (independentVariables.Contains(xi)) // can make this more efficient if necessary
                {
                    derivativeExpressions.Add(total); 
                }
            }
            derivativeExpressions = derivativeExpressions.Reverse().ToList();
            // as final step, we can get rid of any items in the expression tree which do not contribute to derivativeExpressions.
        }

        /// <summary>
        /// Differentiate the function with respect to the variable
        /// </summary>
        /// <param name="function"></param>
        /// <param name="variable"></param>
        /// <returns></returns>
        private static Expression Differentiate(Function function, Expression variable)
        {
            var expression = function.Expression;
            if (expression is BinaryExpression)
            {
                BinaryExpression binaryExpression = expression as BinaryExpression;
                if (expression.NodeType == ExpressionType.Multiply)
                {
                    if (binaryExpression.Left == variable && binaryExpression.Right == variable)
                    {
                        return Expression.Multiply(new ConstantExpression<double>(2), variable);
                    }
                    if (binaryExpression.Left == variable) return binaryExpression.Right;
                    else if (binaryExpression.Right == variable) return binaryExpression.Left; 
                }
            }
            if (expression is ScaleOffsetExpression<double>)
            {
                var scaleOffset = expression as ScaleOffsetExpression<double>;
                return new ConstantExpression<double>(scaleOffset.Scale);
            }
            if (expression is UnaryMathsExpression)
            {
                var unaryExpression = expression as UnaryMathsExpression;
                if (unaryExpression.UnaryType == UnaryElementWiseOperation.Exp) return function.Parameter;
                throw new NotImplementedException();
            }
            else throw new NotImplementedException();
        }

        private static void GetFunctions(VectorBlockExpression block, 
            out Function[] f,
            out List<int>[] jLookup)
        {
            int N = block.ArgumentParameters.Count + block.LocalParameters.Count;
            
            // this stores the indices of the operations which make use of expression i
            jLookup = new List<int>[N];
            for (int i = 0; i < N; ++i) jLookup[i] = new List<int>();
            f = new Function[N];

            // The first set of functions are simply the arguments
            for (int i = 0; i < block.ArgumentParameters.Count; ++i)
            {
                f[i] = new Function() { Expression = block.ArgumentParameters[i], Parameter = block.ArgumentParameters[i] };
            }

            // The next set are the operations
            for (int i = 0; i < block.Operations.Count; ++i)
            {
                var assignment = block.Operations[i];
                if (!(assignment.Left is VectorParameterExpression)) throw new NotImplementedException();

                VectorParameterExpression parameter = assignment.Left as VectorParameterExpression;
                int operationIndex = parameter.Index;
                if (operationIndex != i + block.ArgumentParameters.Count) throw new Exception("index mismatch");
                var operation = assignment.Right;
                f[operationIndex] = new Function() { Expression = assignment.Right, Parameter = parameter };
                if (operation is BinaryExpression)
                {
                    UpdateOperationIndices(jLookup, (operation as BinaryExpression).Left as VectorParameterExpression, 
                        operationIndex);
                    UpdateOperationIndices(jLookup, (operation as BinaryExpression).Right as VectorParameterExpression,
                        operationIndex);
                }
                else if (operation is UnaryMathsExpression)
                {
                    UpdateOperationIndices(jLookup, (operation as UnaryMathsExpression).Operand, operationIndex);
                }
            }
        }

        private static void UpdateOperationIndices(List<int>[] operationIndices, 
            VectorParameterExpression expression, int operationIndex)
        {
            operationIndices[expression.Index].Add(operationIndex);
        }
    }
}
