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
        public bool MultipleThreads = false;
    }
    
    public class VectorOperationRunner
    {
        public static void Execute(DeferredPrimitivesExecutor executor, 
            ILinearAlgebraProvider provider, VectorExecutionOptions vectorOptions)
        {
            //Console.WriteLine(executor.DebugString());
            int chunksLength = 1000;
            var arrayPoolStack = ExecutionContext.ArrayPool.GetStack(chunksLength);

            while (arrayPoolStack.Count < 10)
            {
                arrayPoolStack.Push(new double[chunksLength]);
            }
            
            int length = executor.VectorsLength;

            int chunkCount = length / chunksLength; 
            if (length % chunksLength != 0) chunkCount++;

            AssignNArrayStorage(executor.LocalVariables, chunkCount, chunksLength);

            var options = new ParallelOptions();
            if (!vectorOptions.MultipleThreads) options.MaxDegreeOfParallelism = 1;

            //for (int i = 0; i < chunkCount; i++)
            var operations = Simplify(executor, provider);
    
            Parallel.For(0, chunkCount, options, (i) =>
            {
                int startIndex = i * chunksLength;
                List<double[]> temporaryArrays = new List<double[]>();
                int vectorLength = Math.Min(chunksLength, length - startIndex);
                foreach (var operation in operations)
                {
                    if (operation == null) continue;
                    
                    if (operation is AssignOperation)
                        continue;

                    NArray result;
                    if (IsLocal(operation.Result))
                    {
                        result = operation.Result;
                        var chunkyStorage = result.Storage as ChunkyStorage<double>;
                        temporaryArrays.Add(arrayPoolStack.Pop());
                        chunkyStorage.SetChunk(i, temporaryArrays.Last());
                    }
                    else
                    {
                        result = operation.Result;
                    }
                    if (operation is UnaryVectorOperation)
                    {
                        var unaryOperation = operation as UnaryVectorOperation;

                        unaryOperation.Operation(Slice(unaryOperation.Operand, i, startIndex, vectorLength),
                            Slice(result, i, startIndex, vectorLength));
                    }

                    if (operation is BinaryVectorOperation)
                    {
                        var binaryOperation = operation as BinaryVectorOperation;

                        binaryOperation.Operation(Slice(binaryOperation.Operand1, i, startIndex, vectorLength),
                            Slice(binaryOperation.Operand2, i, startIndex, vectorLength),
                            Slice(result, i, startIndex, vectorLength));
                    }
                }
                foreach (var array in temporaryArrays)
                {
                    arrayPoolStack.Push(array);
                }
            });
            //};
        }

        private static NArray Slice(NArray array, int chunkIndex, int startIndex, int length)
        {
            if (array is LocalNArray)
            {
                return new NArray((array.Storage as ChunkyStorage<double>).Slice(chunkIndex));
            }
            else
            {
                return array.Slice(startIndex, length); 
            }
        }

        private static void AssignNArrayStorage(IList<LocalNArray> localNArrays, int chunkCount, int chunksLength)
        {
            foreach (var localNArray in localNArrays)
            {
                localNArray.Storage = new ChunkyStorage<double>(chunkCount, chunksLength);
            }
        }

        private static bool IsLocal(NArray array)
        {
            return array is LocalNArray;
        }

        private static IList<VectorOperation> Simplify(DeferredPrimitivesExecutor executor, 
            ILinearAlgebraProvider provider)
        {
            var original = executor.VectorOperations;
            //original.ForEach(i => Console.WriteLine(i));
            var simplified = RemoveUnnecessaryLocals(original);

            var simplified2 = SimplifyScaleOffsets(executor, simplified, provider);

            return simplified2;
        }

        private static List<VectorOperation> RemoveUnnecessaryLocals(List<VectorOperation> operations)
        {
            var assignments = operations.OfType<AssignOperation>().Where(o => IsLocal(o.Right) && !IsLocal(o.Left)).ToList();
            Func<NArray, NArray> transform = a => Transform(a, assignments);
            return operations.Where(o => !(o is AssignOperation))
                .Select(o => o.Clone(transform)).ToList(); 
        }

        private static VectorOperation[] SimplifyScaleOffsets(DeferredPrimitivesExecutor executor, List<VectorOperation> operations, ILinearAlgebraProvider provider)
        {
            // one of the most common patterns is
            // local0 = Na * a0 + b0
            // local1 = local0 * a1 + b1
            // Na is an NArray, a0, b0, ... are scalars
            // if local 0 is not subsequently referenced, we can write simply:
            // local1 = Na * [a0 * a1] + [b0 * a1 + b1]
            // this process can then be repeated

            var simplifiedOperations = new VectorOperation[operations.Count];
            var localDefinitionIndex = new int[executor.LocalVariables.Count]; // the index of the operation at which the local is defined

            // Create a look-up of the operation index where a local is defined
            for (int i = 0; i < operations.Count; ++i)
            {
                if (IsLocal(operations[i].Result)) localDefinitionIndex[(operations[i].Result as LocalNArray).Index] = i;
            }
            for (int i = 0; i < operations.Count; ++i)
            {
                // we ignore any ScaleOffsetOperations
                // but for non-ScaleOffsetOperations, where one of the operands is a local, we reinstate its definition, simplified
                var currentOperation = operations[i];
                if (!(currentOperation is ScaleOffsetOperation && IsLocal(currentOperation.Result)))
                {
                    foreach (var operand in currentOperation.Operands())
                    {
                        if (operand is LocalNArray)
                        {
                            var scaleOffsets = new Stack<ScaleOffsetOperation>();
                            // we are going to reinstate this definition, but simplified
                            int operationIndex = localDefinitionIndex[(operand as LocalNArray).Index];
                            var localDefinitionToReplace = operations[operationIndex] as ScaleOffsetOperation;
                            var localDefinition = localDefinitionToReplace;
                            while (localDefinition != null)
                            {
                                scaleOffsets.Push(localDefinition);
                                var scaleOffsetOperand = localDefinition.Operand as LocalNArray;
                                if (scaleOffsetOperand == null) break;
                                localDefinition = operations[localDefinitionIndex[scaleOffsetOperand.Index]] as ScaleOffsetOperation;
                            }
                            if (scaleOffsets.Count > 0)
                            {
                                // we calculate the simplified ScaleOffset
                                double scale = 1; double offset = 0;
                                var finalOperand = scaleOffsets.Peek().Operand;
                                while (scaleOffsets.Count > 0)
                                {
                                    var scaleOffset = scaleOffsets.Pop();
                                    scale *= scaleOffset.Scale;
                                    offset = offset * scaleOffset.Scale + scaleOffset.Offset;
                                }
                                simplifiedOperations[operationIndex] = new ScaleOffsetOperation(finalOperand, localDefinitionToReplace.Result,
                                    scale, offset, provider.ScaleOffset);
                            }
                        }
                    }
                    simplifiedOperations[i] = operations[i];
                }
            }
            return simplifiedOperations;
        }

        private static NArray Transform(NArray original, List<AssignOperation> assignments)
        {
            foreach (var assignment in assignments)
            {
                if (original == assignment.Right) return assignment.Left;
            }
            return original;
        }
    }
}
