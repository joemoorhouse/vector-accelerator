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
    public class CalculationResult 
    {
        NArray[][] _storage; // for each time point, supplies the NArrays for the result and its derivatives

        public CalculationResult(int simulationCount, int timePointCount, int derivativesCount)
        {
            _storage = Enumerable.Range(0, timePointCount)
                .Select(i => Enumerable.Range(0, 1 + derivativesCount)
                        .Select(j => new NArray(StorageLocation.Host, simulationCount, 1)).ToArray())
                            .ToArray();
        }

        public NArray[] GetStorageForTimePoint(int timePoint)
        {
            return _storage[timePoint];
        }
    }
}
