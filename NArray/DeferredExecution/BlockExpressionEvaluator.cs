using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NArray;
using NArray.Interfaces;
using NArray.DeferredExecution;
using NArray.DeferredExecution.Expressions;
using NArray.Storage;

namespace NArray.DeferredExecution
{
    public class VectorExecutionOptions
    {
        public bool MultipleThreads = false;
    }

    public enum Aggregator { None, ElementwiseAdd };

    class BlockExpressionEvaluator
    {
        public static void Evaluate(VectorExecutionOptions options,
            ImmediateNArrayOperationExecutor executor, BlockExpressionBuilder builder,
            NArray[] outputs, NArray dependentVariable, IList<NArray> independentVariables,
            ExecutionTimer timer, StringBuilder expressionsOut = null,
            Aggregator aggregator = Aggregator.ElementwiseAdd)
        {
            if (aggregator != Aggregator.ElementwiseAdd) throw new NotImplementedException();

            var dependentVariableExpression = builder.GetParameter(dependentVariable);
            var independentVariableExpressions = independentVariables
                .Select(v => builder.GetParameter(v)).ToArray();

            // and important special case
            // this can only occur if the dependent variable is a scalar that does not depend on any independent variables
            if (dependentVariableExpression.Index == -1)
            {
                var scalarValue = (dependentVariableExpression as ReferencingVectorParameterExpression).ScalarValue;
                executor.InPlaceAdd(outputs[0], outputs[0], executor.NewScalarNArray(scalarValue));
                // all derivatives remains zero
                return;
            }

            builder.UpdateLocalsNumbering();

            timer.MarkEvaluateSetupComplete();

            // differentiate, extending the block as necessary
            IList<Expression> derivativeExpressions;
            AlgorithmicDifferentiator.Differentiate(builder, out derivativeExpressions,
                dependentVariableExpression, independentVariableExpressions);

            timer.MarkDifferentationComplete();

            // arrange output storage
            var outputIndices = new int[1 + independentVariables.Count];
            bool vectorCalculationsRequired = false;
            for (int i = 0; i < independentVariables.Count + 1; ++i)
            {
                var referencingExpression = (i > 0) ? derivativeExpressions[i - 1] as ReferencingVectorParameterExpression
                    : dependentVariableExpression as ReferencingVectorParameterExpression;
                if (referencingExpression.IsScalar) // common case: constant derivative (especially zero)
                {
                    executor.InPlaceAdd(outputs[i], outputs[i], executor.NewScalarNArray(referencingExpression.ScalarValue));
                    outputIndices[i] = int.MaxValue;
                }
                else // not a scalar so we will need storage 
                {
                    vectorCalculationsRequired = true;
                    outputIndices[i] = referencingExpression.Index;
                    if (outputs[i].IsScalar)
                    {
                        // we increase the storage, copying the scalar value
                        outputs[i] = executor.NewNArray(outputs[i][0], builder.VectorLength, 1);
                    }
                }
            }

            timer.MarkStorageSetupComplete();

            if (vectorCalculationsRequired)
            {
                // put outputs in execution order to be filled:
                var orderedOutputs = new OrderedOutputs(outputs, outputIndices);

                RunNonCompiling(builder, executor.LinearAlgebraProvider,
                    new VectorExecutionOptions(), orderedOutputs, aggregator, timer);
            }

            timer.MarkExecuteComplete();

            if (expressionsOut != null)
            {
                var block = builder.ToBlock();
                expressionsOut.AppendLine("Result in expression "
                    + dependentVariableExpression.ToString());

                if (derivativeExpressions != null && derivativeExpressions.Any())
                {
                    expressionsOut.AppendLine("Derivatives in expressions " + String.Join(", ",
                        derivativeExpressions.Select(e => e.ToString())));
                }
                foreach (var item in block.Operations)
                {
                    var display = item.ToString();
                    expressionsOut.AppendLine(new string(display.ToArray()));
                }
            }
        }

        /// <summary>
        /// Here we execute the expression, but without attempting to compile it.
        /// The performance gain comes only from reducing the amount of memory allocation required, not
        /// from compiling an optimised kernel.
        /// </summary>
        /// <param name="block"></param>
        /// <param name="provider"></param>
        /// <param name="vectorOptions"></param>
        public static void RunNonCompiling(BlockExpressionBuilder _builder,
            ILinearAlgebraProvider provider, VectorExecutionOptions vectorOptions,
            OrderedOutputs orderedOutputs, Aggregator aggregator, ExecutionTimer timer)
        {
            var block = _builder.ToBlock();

            for (int argumentIndex = 0; argumentIndex < block.ArgumentParameters.Count; ++argumentIndex)
            {
                var target = orderedOutputs.TryGetNext(argumentIndex);
                if (target != null) provider.BinaryElementWiseOperation(target.Storage,
                    (block.ArgumentParameters[argumentIndex] as ReferencingVectorParameterExpression).Array.Storage,
                    ExpressionType.Add, target.Storage);
            }

            if (block.Operations.Count == 0) return;

            int chunksLength = _builder.VectorLength; //5000;
            var arrayPoolStack = ExecutionContext.ArrayPool.GetStack(chunksLength);

            int length = _builder.VectorLength;

            int chunkCount = length / chunksLength;
            if (length % chunksLength != 0) chunkCount++;

            List<NArray>[] localsToFree; // the storage that can be freed up after each operation is complete
            AssignNArrayStorage(block, chunkCount, chunksLength, length, out localsToFree);
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
                        ExecuteSingleVectorOperation(newOperation, provider,
                            arrayPoolStack, localsToFree[j],
                            orderedOutputs, aggregator,
                            vectorLength,
                            i, startIndex, timer);

                        foreach (var item in localsToFree[j])
                        {
                            var arrayToReturn = (item.Storage as ChunkyStorage).GetChunk(i);
                            if (arrayToReturn != null) arrayPoolStack.Push(arrayToReturn);
                        }
                    }
                }
            };

            if (!arrayPoolStack.StackCountEqualsCreated) throw new Exception("not all storage arrays created returned to stack");
        }

        private static void ExecuteSingleVectorOperation(BinaryExpression operation,
            ILinearAlgebraProvider provider,
            ArrayPoolStack<double> arrayPoolStack, List<NArray> localsToFree,
            OrderedOutputs orderedOutputs, Aggregator aggregator,
            int vectorLength,
            int chunkIndex, int startIndex, ExecutionTimer timer)
        {
            if (operation == null || operation.NodeType != ExpressionType.Assign) return;

            if (operation == null) return;

            NArray result;
            var left = operation.Left as ReferencingVectorParameterExpression;

            NArray aggregationTarget = null;

            if (left.ParameterType == ParameterType.Local)
            {
                result = left.Array;
                var chunkyStorage = result.Storage as ChunkyStorage;
                if (chunkyStorage != null)
                {
                    var newStorage = arrayPoolStack.Pop();
                    chunkyStorage.SetChunk(chunkIndex, newStorage);
                }
                aggregationTarget = orderedOutputs.TryGetNext(left.Index);
            }
            else
            {
                result = (operation.Left as ReferencingVectorParameterExpression).Array;
            }

            if (operation.Right is UnaryMathsExpression)
            {
                var unaryOperation = operation.Right as UnaryMathsExpression;

                if (unaryOperation.UnaryType == UnaryElementWiseOperations.ScaleOffset)
                {
                    var scaleOffset = unaryOperation as ScaleOffsetExpression;
                    provider.ScaleOffset(Slice(unaryOperation.Operand, chunkIndex, startIndex, vectorLength),
                        scaleOffset.Scale, scaleOffset.Offset,
                        Slice(result.Storage, chunkIndex, startIndex, vectorLength));
                }
                else if (unaryOperation.UnaryType == UnaryElementWiseOperations.ScaleInverse)
                {
                    var scaleInverse = unaryOperation as ScaleInverseExpression;
                    provider.ScaleInverse(Slice(unaryOperation.Operand, chunkIndex, startIndex, vectorLength),
                        scaleInverse.Scale,
                        Slice(result.Storage, chunkIndex, startIndex, vectorLength));
                }
                else
                {
                    provider.UnaryElementWiseOperation(
                        Slice(unaryOperation.Operand, chunkIndex, startIndex, vectorLength),
                        Slice(result.Storage, chunkIndex, startIndex, vectorLength),
                        unaryOperation.UnaryType);
                }
            }

            if (operation.Right is BinaryExpression)
            {
                var binaryOperation = operation.Right as BinaryExpression;
                provider.BinaryElementWiseOperation(Slice(binaryOperation.Left, chunkIndex, startIndex, vectorLength),
                    Slice(binaryOperation.Right, chunkIndex, startIndex, vectorLength),
                    binaryOperation.NodeType,
                    Slice(result.Storage, chunkIndex, startIndex, vectorLength));
            }

            if (aggregationTarget != null)
            {
                if (aggregator != Aggregator.ElementwiseAdd) throw new NotImplementedException();
                if (aggregationTarget.IsScalar) throw new Exception();

                var slice = Slice(aggregationTarget.Storage, chunkIndex, startIndex, vectorLength);
                provider.BinaryElementWiseOperation(
                    slice,
                    Slice(result.Storage, chunkIndex, startIndex, vectorLength),
                    ExpressionType.Add, slice);
            }
        }

        private static INArrayStorage Slice(Expression expression, int chunkIndex, int startIndex, int length)
        {
            var referencingExpression = expression as ReferencingVectorParameterExpression;
            if (referencingExpression == null) throw new NotImplementedException("no support for non-referencing expressions");
            return Slice(GetArray(expression).Storage, chunkIndex, startIndex, length);
        }

        private static INArrayStorage Slice(INArrayStorage storage, int chunkIndex, int startIndex, int length)
        {
            if (storage is ChunkyStorage)
            {
                return (storage as ChunkyStorage).Slice(chunkIndex);
            }
            else
            {
                return new SlicedStorage(storage.Data, storage.DataStart + startIndex, length, 1);
            }
        }

        /// <summary>
        /// Assign ChunkyStorage to locals and determines when each local can be freed-up
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="localsToFree">Contains the idices of the locals that can be freed once the operation is complete</param>
        private static void AssignNArrayStorage(VectorBlockExpression block,
            int chunkCount, int chunksLength, int vectorLength, out List<NArray>[] localsToFree)
        {
            var localNArrays = block.LocalParameters.Select(r => r as ReferencingVectorParameterExpression).
                ToList();

            var lastUseOfLocal = Enumerable.Repeat(block.Operations.Count - 1, block.LocalParameters.Last().Index + 1).ToArray();
            // the operation index that represents the last time a result is used
            int firstLocal = block.LocalParameters.First().Index;
            for (int i = 0; i < block.Operations.Count; ++i)
            {
                foreach (int index in GetOperandIndices(block.Operations[i]))
                {
                    if (index >= firstLocal) lastUseOfLocal[index] = i;
                }
            }
            localsToFree = Enumerable.Range(0, block.Operations.Count)
                .Select(i => new List<NArray>()).ToArray();

            foreach (var localNArray in localNArrays)
            {
                if (localNArray.Array == null) continue;
                localNArray.Array.Storage = new ChunkyStorage(chunkCount, chunksLength);
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

        private static NArray GetArray(Expression expression)
        {
            return (expression as ReferencingVectorParameterExpression).Array;
        }

        private static NArray GetArray(VectorParameterExpression expression)
        {
            return (expression as ReferencingVectorParameterExpression).Array;
        }

        private static IEnumerable<int> GetOperandIndices(Expression expression)
        {
            var right = (expression as BinaryExpression).Right;
            if (right is UnaryMathsExpression)
            {
                yield return ((right as UnaryMathsExpression).Operand as ReferencingVectorParameterExpression).Index;
            }
            else if (right is BinaryExpression)
            {
                var binaryExpression = right as BinaryExpression;
                yield return (binaryExpression.Left as ReferencingVectorParameterExpression).Index;
                yield return (binaryExpression.Right as ReferencingVectorParameterExpression).Index;
            }
            else yield break;
        }
    }

    public class OrderedOutputs
    {
        public int[] OutputsIndices;

        public NArray[] Outputs;

        public int _next;

        public OrderedOutputs(NArray[] outputs, int[] outputsIndices)
        {
            OutputsIndices = (int[])outputsIndices.Clone();
            Outputs = (NArray[])outputs.Clone();
            Array.Sort(OutputsIndices, Outputs);
            _next = 0;
        }

        public NArray TryGetNext(int candidateIndex)
        {
            if (_next < OutputsIndices.Length && candidateIndex == OutputsIndices[_next])
            {
                return Outputs[_next++];
            }
            else return null;
        }
    }
}
