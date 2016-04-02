using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskEngine.Framework;
using VectorAccelerator;
using RiskEngine.Factors;

namespace RiskEngine.Models
{
    [ModelAppliesTo(typeof(MeanRevertingNormal))]
    public class MeanRevertingNormalModel : SnapshotModel<MeanRevertingNormalProcess> { }
    
    [ModelAppliesTo(typeof(MeanRevertingNormalPath))]
    public class MeanRevertingNormalPathModel : PathModel<MeanRevertingNormalProcess> { }

    public class MeanRevertingNormalProcess : Process, IProcess
    {
        public double Sigma { get; set; }

        public double Lambda { get; set; }

        public override void Initialise(Simulation simulation)
        {
            _normalVariates = simulation.RegisterModel<NormalVariates>(Identifier);
            // defaults values:
            Sigma = 0.1; 
            Lambda = 0.05;
        }

        public override NArray Prepare(Context context)
        {
            return context.Factory.CreateNArray(context.Settings.SimulationCount, 1);
        }

        public override NArray Step(TimeInterval timeStep, NArray previous)
        {
            var t = timeStep.IntervalInYears;
            return previous * NMath.Exp(-Lambda * t)
                + E(2.0 * Lambda, t) * _normalVariates.Value;
        }

        private static double E(double lambda, double t)
        {
            return (1.0 - Math.Exp(-lambda * t)) / lambda;
        }

        NormalVariates _normalVariates;
    }
}
