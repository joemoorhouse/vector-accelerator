using System;
using System.Linq;
using VectorAccelerator;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Generic;
using VectorAccelerator.NArrayStorage;
using System.Threading.Tasks;
using VectorAccelerator.Distributions;
using VectorAccelerator.LinearAlgebraProviders;
using VectorAccelerator.DeferredExecution;
using RiskEngine.Framework;
using RiskEngine.Pricers;

namespace RiskEngine.Framework
{
    public static class Calculations
    {
        public static void Calculate(SimulationGraph graph, List<IPricer> pricers, List<NArray> derivatives = null)
        {                    
            var simulationCount = graph.Context.Settings.SimulationCount;
            var timePointCount = graph.Context.Settings.SimulationTimePoints.Length;
            if (derivatives == null) derivatives = new List<NArray>();

            var tasks = new List<Task>();

            foreach (var partition in Partitioner(pricers.Count))
            {
                var resultsStorage = new CalculationResult(simulationCount, timePointCount, derivatives.Count);
                tasks.Add(Task.Run(() =>
                {
                    for (int i = 0; i < timePointCount; ++i)
                    {
                        foreach (var index in partition)
                        {

                            NArray.Evaluate(() =>
                            {
                                NArray pv;
                                pricers[index].Price(i, out pv);
                                return pv;
                            },
                            derivatives, Aggregator.ElementwiseAdd, resultsStorage.GetStorageForTimePoint(i));
                        }
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());
        }

        public static IEnumerable<IEnumerable<int>> Partitioner(int itemCount)
        {
            var tasksCount = Math.Min(itemCount,
                System.Environment.ProcessorCount);

            for (int i = 0; i < tasksCount; ++i)
            {
                yield return SkippingRange(i, itemCount, tasksCount);
            }

            //int itemsPerTask = Math.Max(itemCount / tasksCount, 1);

            //int from = 0;
            //int to = from + itemsPerTask;

            //while (to <= itemCount)
            //{
            //    yield return Enumerable.Range()
            //    from += itemsPerTask;
            //    to = from + itemsPerTask;
            //}
        }  
     
        public static IEnumerable<int> SkippingRange(int from, int to, int skip)
        {
            int i = from;
            while (i < to)
            {
                yield return i;
                i += skip;
            }
        }
    }
}
