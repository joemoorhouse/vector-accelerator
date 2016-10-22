using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskEngine.Framework
{
    public class TimeInterval
    {
        public readonly double IntervalInYears;
        public readonly DateTime Previous;
        public readonly DateTime Next;
        public readonly int Index;

        public TimeInterval(DateTime previous, DateTime next, int index)
        {
            Previous = previous;
            Next = next;
            Index = index;
            IntervalInYears = (next - previous).TotalDays / 365.25;
        }
    }

    public class TimePoint
    {
        public readonly DateTime DateTime;
        public readonly double YearsFromBaseDate;

        public TimePoint(DateTime dateTime, double yearsFromBaseDate)
        {
            DateTime = dateTime;
            YearsFromBaseDate = yearsFromBaseDate;
        }
    }
}
