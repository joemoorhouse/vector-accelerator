using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator;

namespace RiskEngine.Framework
{
    public interface IFactor
    {
        NArray Value { get; }
    }

    public interface INormalVariates : IFactor { }

    public class Factors
    {
        public readonly NArray[] Value;
    }

}
