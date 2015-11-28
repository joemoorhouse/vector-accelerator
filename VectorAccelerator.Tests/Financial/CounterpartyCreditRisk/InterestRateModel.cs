using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorAccelerator.Tests.Financial.CounterpartyCreditRisk
{
    // could have graph where support storage or random access
    // e.g. GetIndex

    public interface IProcess
    {
        //public Factors 
    }

    public class LinearGaussianHJMModel
    {
        public readonly LinearGaussianHJMModel_Factor[] FactorProperties;
        
        public class LinearGaussianHJMModel_Factor
        {
            public double Sigma;
            public double Lambda;
            public string VariatesIdentifier;
        }
    }

    public class InterestRateModel
    {
       
        public void CheckHullWhite()
        {
            double sigma = 0.23;
            double lambda = 0.123;
            double t = 0.25;
            double T = 1.1;

            double a = -(sigma * sigma / lambda) * 
                ( E(lambda, t) * E(lambda, T - t) - E(lambda * 2, t) * E(lambda * 2, T - t) );


            double b = -(sigma * sigma / 2) *
                (E(lambda, T - t) * E(lambda, T - t) * E(2 * lambda, t)
                + E(lambda, t) * E(lambda, t) * E(lambda, T - t));

            double test = a - b;
        }

        public static double E(double lambda, double t)
        {
            return (1.0 - Math.Exp(-lambda * t)) / lambda;
        }
    }
}
