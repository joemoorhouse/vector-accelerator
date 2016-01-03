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
    public class MeanRevertingNormalModel_OLD : SimulatingModel_OLD
    {
        /// <summary>
        /// Volatility
        /// </summary>
        public double Sigma { get { return _sigma; } set { _sigma = value; } }
        
        /// <summary>
        /// Mean version speed
        /// </summary>
        public double Lambda { get { return _lambda; } set { _lambda = value; } }

        public override void Initialise(Simulation simulation)
        {
            _normalVariates = simulation.DefaultModel<INormalVariates>(Identifier);
        }

        internal override void Prepare(Context context)
        {
            _value = context.Factory.CreateNArray(context.Settings.SimulationCount, 1);
        }

        internal override void Step(TimeInterval timeStep)
        {
            var t = timeStep.IntervalInYears;
            _value.Assign(
                _value * NMath.Exp(-_lambda * t) 
                + E(2.0 * _lambda, t) * _normalVariates.Value
            );
        }

        public static double E(double lambda, double t)
        {
            return (1.0 - Math.Exp(-lambda * t)) / lambda;
        }

        private INormalVariates _normalVariates;
        private double _sigma = 0.3;
        private double _lambda = 0.1;
    }
}
