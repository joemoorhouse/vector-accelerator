using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator;

namespace RiskEngine.Framework
{
    public static class Analysis
    {
        public static IEnumerable<double> ApplyMeasures(IEnumerable<Measure> measures, NArray vector)
        {
            var percentileMeasures = measures.OfType<PercentileMeasure>();
            var percentiles = percentileMeasures.Select(m => m.Percentile);
            return NMath.Percentiles(vector, percentiles);
        }
    }
    
    public class Measure
    {
    }

    public class PercentileMeasure : Measure
    {
        public readonly double Percentile;
        
        /// <summary>
        /// Create percentile measure with specified percentile.
        /// </summary>
        /// <param name="percentile">Value of 0.9 means 90%</param>
        public PercentileMeasure(double percentile)
        {
            Percentile = percentile;
        }

        public override string ToString()
        {
            return String.Format("{0:P}", Percentile);
        }

    }

    public class Profile
    {
        public readonly Measure Measure;

        public IReadOnlyCollection<DataPoint> Points
        {
            get { return _dataPoints as IReadOnlyCollection<DataPoint>; }
        }

        public Profile(Measure measure)
        {
            Measure = measure;
        }

        public void AddPoint(DateTime time, double value)
        {
            _dataPoints.Add(new DataPoint(time, value));
        }

        List<DataPoint> _dataPoints = new List<DataPoint>();
    }

    public class SimulationResult
    {
        public readonly Profile[] Profiles; 

        public SimulationResult(IEnumerable<Measure> measures)
        {
            Profiles = measures.Select(m => new Profile(m)).ToArray();
        }

        public void AddValues(DateTime time, params double[] values)
        {
            for (int i = 0; i < values.Length; ++i)
            {
                Profiles[i].AddPoint(time, values[i]);
            }
        }
    }
}
