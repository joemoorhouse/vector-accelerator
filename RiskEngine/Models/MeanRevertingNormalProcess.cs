using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskEngine.Framework;
using VectorAccelerator;

namespace RiskEngine.Models
{
    public class MeanRevertingNormalProcess : SingleFactorProcess
    {
        public double Sigma { get; set; }

        public double Lambda { get; set; }

        public override void Initialise(Simulation simulation)
        {
            _normalVariates = simulation.DefaultModel<INormalVariates>(Identifier);
        }

        internal override void Prepare(Context context)
        {
            Value = context.Factory.CreateNArray(context.Settings.SimulationCount, 1);
        }

        protected override NArray Step(TimeInterval timeStep, NArray previous)
        {
            var t = timeStep.IntervalInYears;
            return previous * NMath.Exp(-Lambda * t)
                + E(2.0 * Lambda, t) * _normalVariates.Value;
        }

        private static double E(double lambda, double t)
        {
            return (1.0 - Math.Exp(-lambda * t)) / lambda;
        }

        INormalVariates _normalVariates;
    }
}
