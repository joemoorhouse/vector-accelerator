using System;
using System.Collections.Generic;
using System.Linq;
using VectorAccelerator.LinearAlgebraProviders;
using VectorAccelerator.NArrayStorage;
using System.Threading.Tasks;

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
        public static void RunNonCompiling(BlockExpressionBuilder _builder, 
            LinearAlgebraProvider provider, VectorExecutionOptions vectorOptions,
            NArray[] outputs, int[] outputsIndices, Aggregator aggregator, ExecutionTimer timer)
        {
            var block = _builder.ToBlock();

            if (block.Operations.Count == 0) return;

            // we will cycle through arrays in order of increasing index
            var getter = new OutputGetter<double>(outputs, outputsIndices);

            int chunksLength = _builder.VectorLength; //5000;
            var arrayPoolStack = ExecutionContext.ArrayPool.GetStack(chunksLength);

            int length = (block.ArgumentParameters.First() as ReferencingVectorParameterExpression<double>)
                .Array.Length;

            int chunkCount = length / chunksLength;
            if (length % chunksLength != 0) chunkCount++;

            List<NArray<double>>[] localsToFree; // the storage that can be freed up after each operation is complete
            AssignNArrayStorage<double>(block, chunkCount, chunksLength, length, out localsToFree);
            // for integer support, add here

            var options = new ParallelOptions();
            if (!vectorOptions.MultipleThreads) options.MaxDegreeOfParallelism = 1;

            timer.MarkExecutionTemporaryStorageAllocationComplete();

            // can multi-thread here, but better to do so at higher level
            //Parallel.For(0, chunkCount, options, (i) =>
            for (int i = 0; i < chunkCount; ++i)
            {
                int startIndex = i * chunksLength;
                int vectorLength = Math.Min(chunksLength, length - startIndex);
                for (int j = 0; j < block.Operations.Count; ++j)
                {
                    var operation = block.Operations[j];
                    if (operation.Type == typeof(NArray))
                    {
                        var newOperation = _builder.SimplifyOperation(operation); // deal with any expressions containing scalars that can be simplified
                        ExecuteSingleVectorOperation<double>(newOperation, provider,
                            arrayPoolStack, localsToFree[j], 
                            getter, aggregator,
                            vectorLength,
                            i, startIndex, timer);
                    }
                }
            };

            if (!arrayPoolStack.StackCountEqualsCreated) throw new Exception("not all storage arrays created returned to stack");
        }

        private static void ExecuteSingleVectorOperation<T>(BinaryExpression operation,
            LinearAlgebraProvider provider,
            ArrayPoolStack<T> arrayPoolStack, List<NArray<T>> localsToFree,
            OutputGetter<T> getter, Aggregator aggregator,
            int vectorLength,
            int chunkIndex, int startIndex, ExecutionTimer timer)
        {            
            if (operation == null || operation.NodeType != ExpressionType.Assign) return;

            if (operation == null) return;

            NArray<T> result;
            var left = operation.Left as ReferencingVectorParameterExpression<T>;

            NArray<T> aggregationTarget = null;

            if (left.ParameterType == ParameterType.Local)
            {
                result = left.Array;
                var chunkyStorage = result.Storage as ChunkyStorage<T>;
                if (chunkyStorage != null)
                {
                    var newStorage = arrayPoolStack.Pop();
                    chunkyStorage.SetChunk(chunkIndex, newStorage);
                }
                aggregationTarget = getter.TryGetNext(left.Index);
            }
            else
            {
                result = (operation.Left as ReferencingVectorParameterExpression<T>).Array;
            }

            if (operation.Right is UnaryMathsExpression)
            {
                var unaryOperation = operation.Right as UnaryMathsExpression;

                if (unaryOperation.UnaryType == UnaryElementWiseOperation.ScaleOffset)
                {
                    var scaleOffset = unaryOperation as ScaleOffsetExpression<T>;
                    (provider as IElementWise<T>).ScaleOffset(Slice<T>(unaryOperation.Operand, chunkIndex, startIndex, vectorLength),
                        scaleOffset.Scale, scaleOffset.Offset,
                        Slice(result, chunkIndex, startIndex, vectorLength));
                }
                else if (unaryOperation.UnaryType == UnaryElementWiseOperation.ScaleInverse)
                {
                    var scaleInverse = unaryOperation as ScaleInverseExpression<T>;
                    (provider as IElementWise<T>).ScaleInverse(Slice<T>(unaryOperation.Operand, chunkIndex, startIndex, vectorLength),
                        scaleInverse.Scale,
                        Slice(result, chunkIndex, startIndex, vectorLength));
                }
                else
                {
                    (provider as IElementWise<T>).UnaryElementWiseOperation(
                        Slice<T>(unaryOperation.Operand, chunkIndex, startIndex, vectorLength),
                        Slice(result, chunkIndex, startIndex, vectorLength),
                        unaryOperation.UnaryType);
                }
            }

            if (operation.Right is BinaryExpression)
            {
                var binaryOperation = operation.Right as BinaryExpression;
                (provider as IElementWise<T>).BinaryElementWiseOperation(Slice<T>(binaryOperation.Left, chunkIndex, startIndex, vectorLength),
                    Slice<T>(binaryOperation.Right, chunkIndex, startIndex, vectorLength),
                    Slice(result, chunkIndex, startIndex, vectorLength),
                    binaryOperation.NodeType);
            }

            if (aggregationTarget != null)
            {
                if (aggregator != Aggregator.ElementwiseAdd) throw new NotImplementedException();
                if (aggregationTarget.IsScalar) throw new Exception();

                var slice = Slice<T>(aggregationTarget, chunkIndex, startIndex, vectorLength);
                (provider as IElementWise<T>).BinaryElementWiseOperation(
                    slice,
                    Slice(result, chunkIndex, startIndex, vectorLength),
                    slice, ExpressionType.Add);
            }
            foreach (var item in localsToFree) arrayPoolStack.Push((item.Storage as ChunkyStorage<T>).GetChunk(chunkIndex));
        }

        private static NArray<T> Slice<T>(Expression expression, int chunkIndex, int startIndex, int length)
        {
            var referencingExpression = expression as ReferencingVectorParameterExpression<T>;
            if (referencingExpression == null) throw new NotImplementedException("no support for non-referencing expressions");
            return Slice(GetArray<T>(expression), chunkIndex, startIndex, length);
        }

        private static NArray<T> Slice<T>(NArray<T> array, int chunkIndex, int startIndex, int length)
        {
            if (array.Storage is ChunkyStorage<T>)
            {
                return NMath.CreateNArray<T>((array.Storage as ChunkyStorage<T>).Slice(chunkIndex));
            }
            else
            {
                return array.Slice(startIndex, length);
            }
        }

        /// <summary>
        /// Assign ChunkyStorage to locals and determines when each local can be freed-up
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="localsToFree">Contains the idices of the locals that can be freed once the operation is complete</param>
        private static void AssignNArrayStorage<T>(VectorBlockExpression block,
            int chunkCount, int chunksLength, int vectorLength, out List<NArray<T>>[] localsToFree)
        {
            var localNArrays = block.LocalParameters.Select(r => r as ReferencingVectorParameterExpression<T>).
                ToList();

            var lastUseOfLocal = Enumerable.Repeat(block.Operations.Count - 1, block.LocalParameters.Last().Index + 1).ToArray(); 
            // the operation index that represents the last time a result is used
            int firstLocal = block.LocalParameters.First().Index;
            for (int i = 0; i < block.Operations.Count; ++i)
            {
                foreach (int index in GetOperandIndices<T>(block.Operations[i]))
                {
                    if (index >= firstLocal) lastUseOfLocal[index] = i;
                } 
            }
            localsToFree = Enumerable.Range(0, block.Operations.Count)
                .Select(i => new List<NArray<T>>()).ToArray();

            foreach (var localNArray in localNArrays)
            {
                if (localNArray.Array == null) continue;
                localNArray.Array.Storage = new ChunkyStorage<T>(chunkCount, chunksLength);
            }

            for (int i = firstLocal; i < lastUseOfLocal.Length; ++i)
            {
                if (lastUseOfLocal[i] >= 0)
                {
                    var local = localNArrays[i - firstLocal];
                    if (local.Array != null) localsToFree[lastUseOfLocal[i]].Add(local.Array);
                }
            }
        }

        private static NArray<T> GetArray<T>(Expression expression)
        {
            return (expression as ReferencingVectorParameterExpression<T>).Array;
        }

        private static NArray<T> GetArray<T>(VectorParameterExpression expression)
        {
            return (expression as ReferencingVectorParameterExpression<T>).Array;
        }

        private static IEnumerable<int> GetOperandIndices<T>(Expression expression)
        {
            var right = (expression as BinaryExpression).Right;
            if (right is UnaryMathsExpression)
            {
                yield return ((right as UnaryMathsExpression).Operand as ReferencingVectorParameterExpression<T>).Index;
            }
            else if (right is BinaryExpression)
            {
                var binaryExpression = right as BinaryExpression;
                yield return (binaryExpression.Left as ReferencingVectorParameterExpression<T>).Index;
                yield return (binaryExpression.Right as ReferencingVectorParameterExpression<T>).Index;
            }
            else yield break;
        }
    }
    public class OutputGetter<T>
    {
        public int[] OutputsIndices;

        public NArray<T>[] Outputs;

        public int _next;

        public OutputGetter(NArray<T>[] outputs, int[] outputsIndices)
        {
            OutputsIndices = (int[])outputsIndices.Clone();
            Outputs = (NArray<T>[])outputs.Clone();
            Array.Sort(OutputsIndices, Outputs);
            _next = 0;
        }

        public NArray<T> TryGetNext(int candidateIndex)
        {
            if (_next < OutputsIndices.Length && candidateIndex == OutputsIndices[_next])
            {
                return Outputs[_next++];
            }
            else return null;
        }
    }
}
