using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator;

namespace RiskEngine.Framework
{
    public class Curve
    {
        public readonly DataPoint[] Data;

        public Curve(IEnumerable<DataPoint> data)
        {
            Data = data.ToArray();
        }

        public DataPoint this[int index]
        {
            get { return Data[index]; }
        }

        public Curve(DateTime[] times, NArray[] values)
        {
            if (times.Length != values.Length) throw new ArgumentException("length mismatch");
            Data = times.Zip(values, (t, v) => new DataPoint(t, v)).ToArray();
        }

        public NArray GetValue(DateTime t)
        {
            int lower = GetLowerIndex(t);
            double weight = (double)(t - Data[lower].Time).TotalDays
                / (double)(Data[lower + 1].Time - Data[lower].Time).TotalDays;
            return weight * Data[lower + 1].Value + (1 - weight) * Data[lower].Value;
        }

        private int GetLowerIndex(DateTime t)
        {
            int lower = 0;
            int upper = Data.Length - 1;
            int mid = (lower + upper) >> 1;
            while ((upper - lower) > 1)
            {
                if (Data[mid].Time >= t)
                    upper = mid;
                else
                    lower = mid;
                mid = (lower + upper) >> 1;
            }
            return lower;
        }
    }
}
