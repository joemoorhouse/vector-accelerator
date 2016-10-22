using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskEngine.Framework;
using VectorAccelerator;
using RiskEngine.Factors;
using RiskEngine.Models;

namespace RiskEngine.Models
{
    [ModelAppliesTo(typeof(NormalVariates))]
    public class WienerModel : SnapshotModel<WienerProcess> { }

    [ModelAppliesTo(typeof(NormalVariatesPath))]
    public class WienerPathModel : PathModel<WienerProcess> { }

    public class WienerProcess : Process, IProcess
    {
        public override void Initialise(SimulationGraph graph)
        {
            _normalVariates = graph.RegisterFactor<NormalVariates>(Identifier);
        }

        public override NArray Prepare(Context context)
        {
            return context.Factory.CreateNArray(context.Settings.SimulationCount, 1);
        }

        public override NArray Step(TimeInterval timeStep, NArray previous)
        {
            var t = timeStep.IntervalInYears;
            return previous + Math.Sqrt(t) * _normalVariates.Value;
        }

        NormalVariates _normalVariates;
    }
}
