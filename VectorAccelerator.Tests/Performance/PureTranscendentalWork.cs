using VectorAccelerator.NArrayStorage;
using VectorAccelerator.Distributions;
using VectorAccelerator.LinearAlgebraProviders;

namespace VectorAccelerator.Tests
{    
    public class PureTranscendentalWork //: IWork
    {
        private double[] GetArray(NArray a)
        {
            return (a.Storage as ManagedStorage<double>).Array;
        }
        
        public void SimpleSpeedTest()
        {
            using (var randomStream = new RandomNumberStream(StorageLocation.Host, RandomNumberGeneratorType.MRG32K3A, 111))
            {
                var normalDistribution = new Normal(randomStream, 0, 1);

                var variates = NArray.CreateRandom(5000, normalDistribution);
                var variatesArray = GetArray(variates);

                var variates2 = NArray.CreateRandom(5000, normalDistribution);
                var variatesArray2 = GetArray(variates);

                var target = NArray.CreateRandom(5000, normalDistribution);

                var targetArray = GetArray(target);

                TestHelpers.Timeit(() =>
                {
                    for (int i = 0; i < 1000; ++i)
                    {
                        //IntelMathKernelLibrary.Multiply(variatesArray, 0, variatesArray2, 0, targetArray, 0, 5000);
                        //IntelMathKernelLibrary.Exp(variatesArray, 0, targetArray, 0, variatesArray.Length);
                        IntelMathKernelLibrary.ConstantAddMultiply(variatesArray, 0, 5, 0, targetArray, 0, 5000);
                    }
                });
            }
        }
        
        //private void DoWork(NArray a, Normal normalDistribution)
        //{
        //    // This will do compute-limited work that we would expect to task parallelize well
        //    var result = NArray.CreateLike(a);
        //    result.Assign(a);
        //    for (int j = 0; j < 100; ++j)
        //    {
        //        //result.FillNormal(random);

        //        var temp = NMath.Exp(result);
        //        result.Assign(NMath.Log(temp));
        //    }
        //}

        //private void DoWorkInPlace(NArray a, Normal normalDistribution)
        //{
        //    // This will do compute-limited work that we would expect to task parallelize well
        //    var result = NArray.CreateLike(a);
        //    result.Assign(a);
        //    for (int j = 0; j < 100; ++j)
        //    {
        //        IntelMathKernelLibrary.Exp(GetArray(result), 0, GetArray(result), 0, result.Length);
        //        IntelMathKernelLibrary.Log(GetArray(result), 0, GetArray(result), 0, result.Length);
        //    }
        //}

        //private void DoWorkDeferred(NArray a, Normal normalDistribution, bool threaded = false)
        //{
        //    // A version where assignment happens, but we defer execution.
        //    var result = NArray.CreateLike(a);
        //    result.Assign(a);

        //    var result2 = NArray.CreateLike(a);

        //    //var options = new DeferredExecution.VectorExecutionOptions() { MultipleThreads = threaded };

        //    for (int j = 0; j < 100; ++j)
        //    {
        //        NArray.Evaluate(() =>
        //        {
        //            return NMath.Log(NMath.Exp(result));
        //        }, new List<NArray>(), Aggregator.ElementwiseAdd, new List<NArray> { result2 });

        //        //using (NArray.DeferredExecution(options))
        //        //{
        //        //    var temp = NMath.Exp(result);
        //        //    result.Assign(NMath.Log(temp));
        //        //}
        //    }
        //}
    }
}
