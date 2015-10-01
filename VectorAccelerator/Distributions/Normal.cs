using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VectorAccelerator.LinearAlgebraProviders;

namespace VectorAccelerator.Distributions
{
    public class Normal : ContinuousDistribution
    {
        public readonly double Mean;
        public readonly double StandardDeviation;
        
        public Normal(RandomNumberStream stream, double mean, double standardDeviation) : base(stream)
        {
            Mean = mean;
            StandardDeviation = standardDeviation;
        }

        public static double NormalPDF(double x)
        {
            return Math.Exp(-x * x / 2) / Math.Sqrt(2 * Math.PI);
        }

        public static double CumulativePDF(double x)
        {
            double k = 1.0 / (1.0 + 0.2316419 * Math.Abs(x));
            double a1 = 0.319381530;
            double a2 = -0.356563782;
            double a3 = 1.781477937;
            double a4 = -1.821255978;
            double a5 = 1.330274429;

            double ret = (a1 * k + a2 * k * k + a3 * k * k * k + a4 * k * k * k * k + a5 * k * k * k * k * k);

            if (x >= 0)
            {
                return 1 - NormalPDF(x) * ret;
            }
            return NormalPDF(-x) * ret;
        }
    }
}
