using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator.Distributions
{
    public abstract class ContinuousDistribution
    {
        public readonly RandomNumberStream RandomNumberStream;

        public ContinuousDistribution(RandomNumberStream stream)
        {
            RandomNumberStream = stream;
        }
    }
}
