using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskEngine.Framework;
using VectorAccelerator;

namespace RiskEngine.Models
{
    /// <summary>
    /// Mean reverting (Ornstein-Uhlenbeck) process with 0 long-run mean, starting at 0.  
    /// </summary>
    public class MeanRevertingNormal : Model
    {
        /// <summary>
        /// Volatility
        /// </summary>
        public double Sigma { get { return _sigma; } set { _sigma = value; } }
        
        /// <summary>
        /// Mean version speed
        /// </summary>
        public double Lambda { get { return _lambda; } set { _lambda = value; } }

        public MeanRevertingNormal(string identifier, ModelFactory factory)
        {
            _normalVariates = factory.CreateDefaultModel<INormalVariates>(identifier);
        }

        public void Initialise()
        {
            _value.Assign(0.0);
        }

        public override void Step(TimeInterval timeStep)
        {
            var t = timeStep.IntervalInYears;
            _value.Assign(
                _value * NMath.Exp(-_lambda * t) 
                + E(2.0 * _lambda, t) * _normalVariates.Value
            );
        }

        public static NArray E(double lambda, double t)
        {
            return (1.0 - NMath.Exp(-lambda * t)) / lambda;
        }

        private INormalVariates _normalVariates;
        private double _sigma = 0.3;
        private double _lambda = 0.1;
    }
}
