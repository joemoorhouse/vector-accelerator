using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskEngine.Framework
{
    public class State<T>
    {
        public readonly DateTime[] SimulationPoints;

        public readonly T[] Values;

        public static int BinarySearch(DateTime[] dates, DateTime dateToFind)
        {
            int left = 0;
            int right = dates.Length - 1;
            int mid = (left + right) >> 1;
            while ((right - left) > 1)
            {
                if (dates[mid] >= dateToFind)
                    right = mid;
                else
                    left = mid;
                mid = (left + right) >> 1;
            }
            return left;
        }

        public T GetState(DateTime date)
        {
            return Values[BinarySearch(SimulationPoints, date)];
        }

        public T GetState(int timeIndex)
        {
            return Values[timeIndex];
        }
    }

}
