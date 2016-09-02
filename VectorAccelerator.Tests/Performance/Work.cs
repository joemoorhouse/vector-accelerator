using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VectorAccelerator;
using VectorAccelerator.NArrayStorage;
using VectorAccelerator.Distributions;
using VectorAccelerator.LinearAlgebraProviders;

namespace VectorAccelerator.Tests.Performance
{
    public interface IWork
    {  
        void CreateStorage();

        void DoManagedWork();

        void DoWorkInPlace();

        void DoWorkImmediate();

        void DoWorkDeferred();
    }
    
    public abstract class Work
    {
        /// <summary>
        /// Input 1
        /// </summary>
        NArray _a;
        double[] _a_array;
        /// <summary>
        /// Input 2
        /// </summary>
        NArray _b;
        double[] _b_array;
        /// <summary>
        /// Input 3
        /// </summary>
        NArray _c;
        double[] _c_array;

        /// <summary>
        /// Output
        /// </summary>
        NArray _y;
        double[] _y_array;

        public void CreateInputs()
        {
            var location = StorageLocation.Host;

            using (var randomStream = new RandomNumberStream(location, RandomNumberGeneratorType.MRG32K3A, 111))
            {
                var normalDistribution = new Normal(randomStream, 0, 1);

                _a = new NArray(location, 5000);
                _a.FillRandom(normalDistribution);
                _a_array = GetArray(_a);

                _b = new NArray(location, 5000);
                _b.FillRandom(normalDistribution);
                _b_array = GetArray(_b);

                _c = new NArray(location, 5000);
                _c.FillRandom(normalDistribution);
                _c_array = GetArray(_c);
            }
        }

        public void ClearResult()
        {

        }

        private double[] GetArray(NArray a)
        {
            return (a.Storage as ManagedStorage<double>).Array;
        }
    }
}
