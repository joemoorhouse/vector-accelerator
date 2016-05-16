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

        public TimeInterval(DateTime previous, DateTime next)
        {
            Previous = previous;
            Next = next;
            IntervalInYears = (next - previous).Days / 365.25;
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
