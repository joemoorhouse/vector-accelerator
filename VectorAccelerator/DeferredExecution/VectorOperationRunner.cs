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

            AssignNArrayStorage(executor.LocalVariables.OfType<NArray<double>>(), chunkCount, chunksLength);
            // and integers too?

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
                    if (operation is NArrayOperation<double>)
                    {
                        ExecuteSingleVectorOperation<double>(operation as NArrayOperation<double>,
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
            //};
        }

        private static void ExecuteSingleVectorOperation<T>(NArrayOperation<T> operation, 
            ArrayPoolStack<T> arrayPoolStack, List<T[]> temporaryArrays,
            int vectorLength,
            int chunkIndex, int startIndex)
        {
            if (operation == null) return;

            if (operation is AssignOperation<T>)
                return;

            NArray<T> result;
            if (IsLocal(operation.Result))
            {
                result = operation.Result;
                var chunkyStorage = result.Storage as ChunkyStorage<T>;
                temporaryArrays.Add(arrayPoolStack.Pop());
                chunkyStorage.SetChunk(chunkIndex, temporaryArrays.Last());
            }
            else
            {
                result = operation.Result;
            }

            if (operation is UnaryVectorOperation<T>)
            {
                var unaryOperation = operation as UnaryVectorOperation<T>;

                unaryOperation.Operation(Slice(unaryOperation.Operand, chunkIndex, startIndex, vectorLength),
                    Slice(result, chunkIndex, startIndex, vectorLength));
            }

            if (operation is BinaryVectorOperation<T>)
            {
                var binaryOperation = operation as BinaryVectorOperation<T>;

                binaryOperation.Operation(Slice(binaryOperation.Operand1, chunkIndex, startIndex, vectorLength),
                    Slice(binaryOperation.Operand2, chunkIndex, startIndex, vectorLength),
                    Slice(result, chunkIndex, startIndex, vectorLength));
            }
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

        private static bool IsLocal<T>(NArray<T> array)
        {
            return array is ILocalNArray;
        }

        private static IList<NArrayOperation> Simplify(DeferredPrimitivesExecutor executor,
            ILinearAlgebraProvider provider)
        {
            var operations = new List<NArrayOperation>();
            operations.AddRange(executor.VectorOperations);
            RemoveUnnecessaryLocals(operations);
            return operations;
        }
        //    var original = executor.VectorOperations;
        //    //original.ForEach(i => Console.WriteLine(i));
        //    var simplified = RemoveUnnecessaryLocals(original);

        //    var simplified2 = SimplifyScaleOffsets(executor, simplified, provider);

        //    return simplified2;
        //}

        private static void RemoveUnnecessaryLocals(List<NArrayOperation> operations)
        {
            var assignments = operations.OfType<AssignOperation<double>>().Where(o => IsLocal(o.Right) && !IsLocal(o.Left)).ToList();
            Func<NArray<double>, NArray<double>> transform = a => Transform(a, assignments);
            
            for (int i = 0; i < operations.Count; ++i)
            {
                if (operations[i] is AssignOperation<double>)
                {
                    operations[i] = null;
                }
                else
                {
                    var operation = operations[i] as NArrayOperation<double>;
                    if (operation != null)
                    {
                        operations[i] = operation.Clone(transform);
                    }
                }
            }
        }

        //private static NArrayOperation[] SimplifyScaleOffsets(DeferredPrimitivesExecutor executor, List<NArrayOperation> operations, ILinearAlgebraProvider provider)
        //{
        //    // one of the most common patterns is
        //    // local0 = Na * a0 + b0
        //    // local1 = local0 * a1 + b1
        //    // Na is an NArray, a0, b0, ... are scalars
        //    // if local 0 is not subsequently referenced, we can write simply:
        //    // local1 = Na * [a0 * a1] + [b0 * a1 + b1]
        //    // this process can then be repeated

        //    var simplifiedOperations = new NArrayOperation[operations.Count];
        //    var localDefinitionIndex = new int[executor.LocalVariables.Count]; // the index of the operation at which the local is defined

        //    // Create a look-up of the operation index where a local is defined
        //    for (int i = 0; i < operations.Count; ++i)
        //    {
        //        if (IsLocal(operations[i].Result)) localDefinitionIndex[(operations[i].Result as LocalNArray).Index] = i;
        //    }
        //    for (int i = 0; i < operations.Count; ++i)
        //    {
        //        // we ignore any ScaleOffsetOperations
        //        // but for non-ScaleOffsetOperations, where one of the operands is a local, we reinstate its definition, simplified
        //        var currentOperation = operations[i];
        //        if (!(currentOperation is ScaleOffsetOperation && IsLocal(currentOperation.Result)))
        //        {
        //            foreach (var operand in currentOperation.Operands())
        //            {
        //                if (operand is LocalNArray)
        //                {
        //                    var scaleOffsets = new Stack<ScaleOffsetOperation>();
        //                    // we are going to reinstate this definition, but simplified
        //                    int operationIndex = localDefinitionIndex[(operand as LocalNArray).Index];
        //                    var localDefinitionToReplace = operations[operationIndex] as ScaleOffsetOperation;
        //                    var localDefinition = localDefinitionToReplace;
        //                    while (localDefinition != null)
        //                    {
        //                        scaleOffsets.Push(localDefinition);
        //                        var scaleOffsetOperand = localDefinition.Operand as LocalNArray;
        //                        if (scaleOffsetOperand == null) break;
        //                        localDefinition = operations[localDefinitionIndex[scaleOffsetOperand.Index]] as ScaleOffsetOperation;
        //                    }
        //                    if (scaleOffsets.Count > 0)
        //                    {
        //                        // we calculate the simplified ScaleOffset
        //                        double scale = 1; double offset = 0;
        //                        var finalOperand = scaleOffsets.Peek().Operand;
        //                        while (scaleOffsets.Count > 0)
        //                        {
        //                            var scaleOffset = scaleOffsets.Pop();
        //                            scale *= scaleOffset.Scale;
        //                            offset = offset * scaleOffset.Scale + scaleOffset.Offset;
        //                        }
        //                        simplifiedOperations[operationIndex] = new ScaleOffsetOperation(finalOperand, localDefinitionToReplace.Result,
        //                            scale, offset, provider.ScaleOffset);
        //                    }
        //                }
        //            }
        //            simplifiedOperations[i] = operations[i];
        //        }
        //    }
        //    return simplifiedOperations;
        //}

        private static NArray<T> Transform<T>(NArray<T> original, List<AssignOperation<T>> assignments)
        {
            foreach (var assignment in assignments)
            {
                if (original == assignment.Right) return assignment.Left;
            }
            return original;
        }
    }
}
