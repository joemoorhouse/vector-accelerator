using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VectorAccelerator.LinearAlgebraProviders;
using VectorAccelerator.NArrayStorage;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace VectorAccelerator.DeferredExecution.Expressions
{
    class VectorBlockExpressionRunner
    {
        /// <summary>
        /// Here we execute the expression, but without attempting to compile it.
        /// The performance gain comes only from reducing the amount of memory allocation required, not
        /// from compiling an optimised kernel.
        /// </summary>
        /// <param name="block"></param>
        /// <param name="provider"></param>
        /// <param name="vectorOptions"></param>
        public static void RunNonCompiling(VectorBlockExpression block, 
            LinearAlgebraProvider provider, VectorExecutionOptions vectorOptions)
        {
            //Console.WriteLine(executor.DebugString());
            int chunksLength = 1000;
            var arrayPoolStack = ExecutionContext.ArrayPool.GetStack(chunksLength);

            while (arrayPoolStack.Count < 10)
            {
                arrayPoolStack.Push(new double[chunksLength]);
            }

            int length = block.ArgumentParameters.OfType<NArray<double>>().First().Length;

            int chunkCount = length / chunksLength;
            if (length % chunksLength != 0) chunkCount++;

            //AssignNArrayStorage(executor.LocalVariables.OfType<NArray<double>>(), chunkCount, chunksLength);
            // and integers too?

            var options = new ParallelOptions();
            if (!vectorOptions.MultipleThreads) options.MaxDegreeOfParallelism = 1;

            //var operations = Simplify(executor, provider);

            Parallel.For(0, chunkCount, options, (i) =>
            {
                int startIndex = i * chunksLength;
                List<double[]> temporaryArrays = new List<double[]>();
                int vectorLength = Math.Min(chunksLength, length - startIndex);
                foreach (var operation in block.Operations)
                {
                    if (operation.Type == typeof(NArray<double>))
                    {
                        ExecuteSingleVectorOperation<double>(operation, provider,
                            arrayPoolStack, temporaryArrays,
                            vectorLength,
                            i, startIndex);
                    }
                }
                foreach (var array in temporaryArrays)
                {
                    arrayPoolStack.Push(array);
                }
            });
        }

        private static void ExecuteSingleVectorOperation<T>(BinaryExpression operation,
            LinearAlgebraProvider provider,
            ArrayPoolStack<T> arrayPoolStack, List<T[]> temporaryArrays,
            int vectorLength,
            int chunkIndex, int startIndex)
        {
            if (operation.NodeType != ExpressionType.Assign) return;
            
            if (operation == null) return;

            if (operation is AssignOperation<T>)
                return;

            NArray<T> result;
            if (operation.Left is LocalNArrayVectorParameterExpression<T>)
            {
                result = (operation.Left as LocalNArrayVectorParameterExpression<T>).Array;
                var chunkyStorage = result.Storage as ChunkyStorage<T>;
                temporaryArrays.Add(arrayPoolStack.Pop());
                chunkyStorage.SetChunk(chunkIndex, temporaryArrays.Last());
            }
            else
            {
                result = (operation.Left as ReferencingVectorParameterExpression<T>).Array;
            }

            if (operation.Right is UnaryMathsExpression)
            {
                var unaryOperation = operation.Right as UnaryMathsExpression;

                (provider as IElementWise<T>).UnaryElementWiseOperation(
                    Slice<T>(unaryOperation.Operand, chunkIndex, startIndex, vectorLength),
                    Slice(result, chunkIndex, startIndex, vectorLength),
                    unaryOperation.UnaryType);

                //var unaryOperation = operation as UnaryVectorOperation<T>;

                //unaryOperation.Operation(Slice(unaryOperation.Operand, chunkIndex, startIndex, vectorLength),
                //    Slice(result, chunkIndex, startIndex, vectorLength));
            }

            if (operation is BinaryVectorOperation<T>)
            {
                var binaryOperation = operation as BinaryVectorOperation<T>;

                binaryOperation.Operation(Slice(binaryOperation.Operand1, chunkIndex, startIndex, vectorLength),
                    Slice(binaryOperation.Operand2, chunkIndex, startIndex, vectorLength),
                    Slice(result, chunkIndex, startIndex, vectorLength));
            }
        }

        private static NArray<T> Slice<T>(VectorParameterExpression expression, int chunkIndex, int startIndex, int length)
        {
            var referencingExpression = expression as ReferencingVectorParameterExpression<T>;
            if (referencingExpression == null) throw new NotImplementedException("no support for non-referencing expressions");
            return Slice(GetArray<T>(expression), chunkIndex, startIndex, length);
        }

        private static NArray<T> Slice<T>(NArray<T> array, int chunkIndex, int startIndex, int length)
        {
            if (array is ILocalNArray)
            {
                return NMath.CreateNArray<T>((array.Storage as ChunkyStorage<T>).Slice(chunkIndex));
            }
            else
            {
                return array.Slice(startIndex, length);
            }
        }

        private static void AssignNArrayStorage<T>(IEnumerable<NArray<T>> localNArrays, int chunkCount, int chunksLength)
        {
            foreach (var localNArray in localNArrays)
            {
                localNArray.Storage = new ChunkyStorage<T>(chunkCount, chunksLength);
            }
        }

        private static NArray<T> GetArray<T>(VectorParameterExpression expression)
        {
            return (expression as ReferencingVectorParameterExpression<T>).Array;
        }

        private static bool IsLocal<T>(NArray<T> array)
        {
            return array is ILocalNArray;
        }
    }
}
