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
            if (independentVariables.Length == 0)
            {
                derivativeExpressions = null;
                return;
            }
            
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

            bool[] derivativeRequired; int[] derivativeExpressionIndex;
            IdentifyNeccesaryDerivatives(N, jLookup, independentVariables, out derivativeRequired, out derivativeExpressionIndex);

            // the list of operations needed to calculate the derivatives (*all* derivatives)
            derivativeExpressions = new Expression[independentVariables.Length]; 
            var dxNdx = new Expression[N + 1];
            dxNdx[N] = new ConstantExpression<double>(1.0); 
            for (int i = N - 1; i >= 0; i--)
            {
                if (!derivativeRequired[i]) continue;
                // dxN / dxi
                // need to find all operation indices j such that p(j) contains i
                // that is, all functions that have xi as an argument (these must therefore have a higher index than i) 
                VectorParameterExpression total = new ConstantExpression<double>(0);
                var xi = f[i].Parameter;
                //need sum of dfj/dxi dXN/dxj
                foreach (var j in jLookup[i])
                {
                    var fj = f[j];
                    var dfjdxi = Differentiate(fj, xi, builder); // dfj/dxi 
                    var dXNdxj = dxNdx[j]; // dXN/dxj
                    var product = builder.AddProductExpression(dfjdxi, dXNdxj);
                    total = builder.AddAdditionExpression(total, product);
                }
                dxNdx[i] = total;
                int targetIndex = derivativeExpressionIndex[i];
                if (targetIndex != -1) derivativeExpressions[targetIndex] = total;
            }
        }

        private static void IdentifyNeccesaryDerivatives(int N, List<int>[] jLookup,
            VectorParameterExpression[] independentVariables,
            out bool[] derivativeRequired, out int[] derivativeExpressionIndex)
        {
            derivativeRequired = new bool[N + 1]; // true if the derivative expression is required
            derivativeExpressionIndex = Enumerable.Repeat(-1, N + 1).ToArray();
            // the index of the independent variable that the derivative expression refers to
            for (int i = 0; i < independentVariables.Length; ++i)
            {
                int expessionindex = independentVariables[i].Index;
                derivativeExpressionIndex[expessionindex] = i;
                derivativeRequired[expessionindex] = true;
            }
            for (int i = 0; i < N; i++)
            {
                if (derivativeRequired[i])
                {
                    foreach (int j in jLookup[i])
                    {
                        derivativeRequired[j] = true;
                    }
                }
            }
        }

        /// <summary>
        /// Differentiate the function with respect to the variable
        /// </summary>
        /// <param name="function"></param>
        /// <param name="variable"></param>
        /// <returns></returns>
        private static Expression Differentiate(Function function, Expression variable, BlockExpressionBuilder builder)
        {
            // note that one of the function parameters is the variable
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
                else if (expression.NodeType == ExpressionType.Add)
                {
                    if (binaryExpression.Left == variable && binaryExpression.Right == variable)
                    {
                        return new ConstantExpression<double>(2);
                    }
                    else if (binaryExpression.Left == variable) return new ConstantExpression<double>(1);
                    else if (binaryExpression.Right == variable) return new ConstantExpression<double>(1);
                }
                else if (expression.NodeType == ExpressionType.Subtract)
                {
                    if (binaryExpression.Left == variable && binaryExpression.Right == variable)
                    {
                        return new ConstantExpression<double>(0);
                    }
                    else if (binaryExpression.Left == variable) return new ConstantExpression<double>(1);
                    else if (binaryExpression.Right == variable) return new ConstantExpression<double>(-1);
                }
                else if (expression.NodeType == ExpressionType.Divide)
                {
                    if (binaryExpression.Left == variable && binaryExpression.Right == variable) throw new NotImplementedException(); // should not happen
                    if (binaryExpression.Left == variable)
                    {
                        var right = binaryExpression.Right as ReferencingVectorParameterExpression<double>;
                        if (right.IsScalar) return builder.AddLocalAssignment<double>(1 / right.ScalarValue, new ScaleInverseExpression<double>(binaryExpression.Right as VectorParameterExpression, 1));
                        else return builder.AddLocalAssignment<double>(new ScaleInverseExpression<double>(binaryExpression.Right as VectorParameterExpression, 1));
                    }
                    else if (binaryExpression.Right == variable)
                    {
                        return builder.AddNegateDivideExpression(function.Parameter, binaryExpression.Right as VectorParameterExpression); // i.e. x/y => x/y * -1/y 
                    }
                }
            }

            if (expression is UnaryMathsExpression)
            {
                var unaryExpression = expression as UnaryMathsExpression;
                if (unaryExpression.UnaryType == UnaryElementWiseOperation.ScaleOffset) 
                {
                    return new ConstantExpression<double>((unaryExpression as ScaleOffsetExpression<double>).Scale);
                }
                else if (unaryExpression.UnaryType == UnaryElementWiseOperation.ScaleInverse) 
                {
                    return builder.AddNegateDivideExpression(function.Parameter, unaryExpression.Operand); // i.e. x/y => x/y * -1/y 
                }
                else if (unaryExpression.UnaryType == UnaryElementWiseOperation.Exp) return function.Parameter;
                else if (unaryExpression.UnaryType == UnaryElementWiseOperation.Log) return builder.AddInverseExpression(unaryExpression.Operand);
                else if (unaryExpression.UnaryType == UnaryElementWiseOperation.SquareRoot) return builder.AddHalfInverseSquareRootExpression(unaryExpression.Operand);
                else if (unaryExpression.UnaryType == UnaryElementWiseOperation.CumulativeNormal) return builder.AddGaussian(unaryExpression.Operand);
                else throw new NotImplementedException();
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
                //if (operationIndex != i + block.ArgumentParameters.Count) throw new Exception("index mismatch");
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
            if (expression.Index != -1) operationIndices[expression.Index].Add(operationIndex); // -1 indices a constant
        }
    }
}
