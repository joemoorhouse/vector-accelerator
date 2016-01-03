using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskEngine
{
    public struct DataPoint
    {
        public readonly DateTime Time;
        public readonly double Value;

        public DataPoint(DateTime time, double value)
        {
            Time = time; Value = value;
        }
    }
}
