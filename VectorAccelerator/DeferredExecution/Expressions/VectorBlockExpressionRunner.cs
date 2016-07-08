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
        public static void RunNonCompiling(BlockExpressionBuilder _builder, 
            LinearAlgebraProvider provider, VectorExecutionOptions vectorOptions,
            NArray[] outputs, int[] outputsIndices, Aggregator aggregator)
        {
            var block = _builder.ToBlock();

            if (block.Operations.Count == 0) return;

            // we will cycle through arrays in order of increasing index
            var getter = new OutputGetter<double>(outputs, outputsIndices);

            int chunksLength = _builder.VectorLength; //5000;
            var arrayPoolStack = ExecutionContext.ArrayPool.GetStack(chunksLength);

            while (arrayPoolStack.Count < 10)
            {
                arrayPoolStack.Push(new double[chunksLength]);
            }

            int length = (block.ArgumentParameters.First() as ReferencingVectorParameterExpression<double>)
                .Array.Length;

            int chunkCount = length / chunksLength;
            if (length % chunksLength != 0) chunkCount++;

            AssignNArrayStorage<double>(block, chunkCount, chunksLength, length);
            // and integers too?

            var options = new ParallelOptions();
            if (!vectorOptions.MultipleThreads) options.MaxDegreeOfParallelism = 1;

            //var operations = Simplify(executor, provider);

            //Parallel.For(0, chunkCount, options, (i) =>
            for (int i = 0; i < chunkCount; ++i)
            {
                int startIndex = i * chunksLength;
                List<double[]> temporaryArrays = new List<double[]>();
                int vectorLength = Math.Min(chunksLength, length - startIndex);
                foreach (var operation in block.Operations)
                {
                    if (operation.Type == typeof(NArray))
                    {
                        var newOperation = _builder.SimplifyOperation(operation); // deal with any expressions containing scalars that can be simplified
                        ExecuteSingleVectorOperation<double>(newOperation, provider,
                            arrayPoolStack, temporaryArrays, 
                            getter, aggregator,
                            vectorLength,
                            i, startIndex);
                    }
                }
                foreach (var array in temporaryArrays)
                {
                    arrayPoolStack.Push(array);
                }
            };//);
        }

        private static void ExecuteSingleVectorOperation<T>(BinaryExpression operation,
            LinearAlgebraProvider provider,
            ArrayPoolStack<T> arrayPoolStack, List<T[]> temporaryArrays, 
            OutputGetter<T> getter, Aggregator aggregator,
            int vectorLength,
            int chunkIndex, int startIndex)
        {   
            if (operation == null || operation.NodeType != ExpressionType.Assign) return;

            if (operation == null) return;

            if (operation is AssignOperation<T>)
                return;

            NArray<T> result;
            var left = operation.Left as ReferencingVectorParameterExpression<T>;

            NArray<T> aggregationTarget = null;

            if (left.ParameterType == ParameterType.Local)
            {
                result = left.Array;
                var chunkyStorage = result.Storage as ChunkyStorage<T>;
                if (chunkyStorage != null)
                {
                    temporaryArrays.Add(arrayPoolStack.Pop());
                    chunkyStorage.SetChunk(chunkIndex, temporaryArrays.Last());
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

        private static void AssignNArrayStorage<T>(VectorBlockExpression block,
            int chunkCount, int chunksLength, int vectorLength)
        {
            var localNArrays = block.LocalParameters.Select(r => (r as ReferencingVectorParameterExpression<T>).Array);

            foreach (var localNArray in localNArrays)
            {
                if (localNArray == null) continue;
                localNArray.Storage = new ChunkyStorage<T>(chunkCount, chunksLength);
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
