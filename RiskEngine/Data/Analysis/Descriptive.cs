using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator;
using VectorAccelerator.NArrayStorage;

namespace RiskEngine.Data
{
    public static class Descriptive
    {
        public static IEnumerable<double> ToEnumerable(this NArray array)
        {
            var storage = array.Storage as ManagedStorage<double>;
            return storage.Array.Skip(storage.ArrayStart).Take(storage.Length);
        }

        public static double Correlation(NArray returns1, NArray returns2)
        {
            return Correlation(returns1.ToEnumerable(), returns2.ToEnumerable());
        }

        public static double Correlation(IEnumerable<double> returns1, IEnumerable<double> returns2)
        {
            double variance1, variance2;
            int count;
            double covariance = Covariance(returns1, returns2, out variance1, out variance2, out count);
            return covariance / Math.Sqrt(variance1 * variance2);
        }

        public static double Covariance(IEnumerable<double> returns1, IEnumerable<double> returns2, out double variance1, out double variance2, out int count)
        {
            var iter1 = returns1.GetEnumerator();
            var iter2 = returns2.GetEnumerator();
            double sum1 = 0, sum2 = 0;
            double sum = 0;
            count = 0;
            while (iter1.MoveNext() && iter2.MoveNext())
            {
                if (!Double.IsNaN(iter1.Current) && !Double.IsNaN(iter2.Current))
                {
                    sum1 += iter1.Current; sum2 += iter2.Current;
                    count++;
                }
            }
            double mean1 = sum1 / count;
            double mean2 = sum2 / count;
            iter1 = returns1.GetEnumerator();
            iter2 = returns2.GetEnumerator();
            sum1 = 0; sum2 = 0;
            while (iter1.MoveNext() && iter2.MoveNext())
            {
                // Do not include any NaNs in the calculation. 
                if (!Double.IsNaN(iter1.Current) && !Double.IsNaN(iter2.Current))
                {
                    double diff1 = iter1.Current - mean1;
                    double diff2 = iter2.Current - mean2;
                    sum += diff1 * diff2;
                    sum1 += diff1 * diff1;
                    sum2 += diff2 * diff2;
                }
            }
            if (count == 0)
            {
                variance1 = variance2 = Double.NaN; return Double.NaN;
            }
            variance1 = sum1 / count; variance2 = sum2 / count;
            return sum / count;
        }

        public static double StdDev(IEnumerable<double> returns)
        {
            double variance1, variance2;
            int count;
            return Math.Sqrt(Covariance(returns, returns, out variance1, out variance2, out count));
        }
    }
}
