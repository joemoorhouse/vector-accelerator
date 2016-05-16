using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator;

namespace RiskEngine.Framework
{
    public struct DataPoint
    {
        public readonly DateTime Time;
        public readonly NArray Value;

        public DataPoint(DateTime time, NArray value)
        {
            Time = time; Value = value;
        }

        public override string ToString()
        {
            return string.Join(" ", Time.ToShortDateString(), Value.ToString());
        }
    }
}
