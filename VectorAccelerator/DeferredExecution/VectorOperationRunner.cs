using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VectorAccelerator.LinearAlgebraProviders;
using VectorAccelerator.NArrayStorage;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace VectorAccelerator.DeferredExecution
{
    public class VectorExecutionOptions
    {
        public bool MultipleThreads = true;
    }
    
    public class VectorOperationRunner
    {
        public static void Execute(DeferredPrimitivesExecutor executor, 
            ILinearAlgebraProvider provider, VectorExecutionOptions vectorOptions)
        {
            int chunksLength = 256;
            var arrayPoolStack = ExecutionContext.ArrayPool.GetStack(chunksLength);

            while (arrayPoolStack.Count < 10)
            {
                arrayPoolStack.Push(new double[chunksLength]);
            }
            
            int length = executor.VectorsLength;
            int chunkIndex = 0;
            int chunkCount = length / chunksLength; 
            if (length % chunksLength != 0) chunkCount++;

            var aliases = GetLocalNArrayAliases(executor);

            var options = new ParallelOptions();
            if (!vectorOptions.MultipleThreads) options.MaxDegreeOfParallelism = 1;

            //for (int i = 0; i < chunkCount; i++)
            Parallel.For(0, chunkCount, options, (i) =>
            {
                int startIndex = i * chunksLength;
                List<double[]> temporaryArrays = new List<double[]>();
                int vectorLength = Math.Min(chunksLength, length - startIndex);
                foreach (var operation in executor.VectorOperations)
                {
                    if (operation is AssignOperation)
                        continue;

                    NArray result;
                    if (IsLocal(operation.Result) && aliases[(operation.Result as LocalNArray).Index] != null)
                    {
                        result = aliases[(operation.Result as LocalNArray).Index]
                            .Slice(startIndex, vectorLength);
                    }
                    else
                    {
                        result = operation.Result;
                        temporaryArrays.Add(arrayPoolStack.Pop());
                        result.Storage = new ManagedStorage<double>(temporaryArrays.Last());
                    }
                    if (operation is UnaryVectorOperation)
                    {
                        var unaryOperation = operation as UnaryVectorOperation;

                        unaryOperation.Operation(Slice(unaryOperation.Operand, startIndex, vectorLength),
                            result);
                    }

                    if (operation is BinaryVectorOperation)
                    {
                        var binaryOperation = operation as BinaryVectorOperation;

                        binaryOperation.Operation(Slice(binaryOperation.Operand1, startIndex, vectorLength),
                            Slice(binaryOperation.Operand2, startIndex, vectorLength),
                            result);
                    }
                }
                foreach (var array in temporaryArrays)
                {
                    arrayPoolStack.Push(array);
                }
                chunkIndex++;
            });
            //};
        }

        private static NArray Slice(NArray array, int startIndex, int length)
        {
            if (array is LocalNArray)
            {
                return array;
            }
            else
            {
                return array.Slice(startIndex, length); 
            }
        }

        /// <summary>
        /// In many cases it is not necessary to create temporary storage: a vector operation can write straight 
        /// to an output.
        /// </summary>
        /// <returns></returns>
        private static NArray[] GetLocalNArrayAliases(DeferredPrimitivesExecutor executor)
        {
            var aliases = new NArray[executor.LocalVariables.Count];
            foreach (var assignment in executor.VectorOperations.OfType<AssignOperation>())
            {
                if (IsLocal(assignment.Right) && !IsLocal(assignment.Left))
                {
                    aliases[(assignment.Right as LocalNArray).Index] = assignment.Left;
                }
            }
            return aliases;
        }

        private static bool IsLocal(NArray array)
        {
            return array is LocalNArray;
        }
    }
}
