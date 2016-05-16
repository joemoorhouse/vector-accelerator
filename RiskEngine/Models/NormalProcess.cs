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
    public class NormalModel : SnapshotModel<NormalProcess> { }

    //[ModelAppliesTo(typeof(NormalVariatesPath))]
    public class NormalPathModel : PathModel<NormalProcess> { }

    public class NormalProcess : Process, IProcess
    {
        public override void Initialise(SimulationGraph graph)
        {
            // we get the (single) instance of the MultiVariateNormalModel
            var model = graph.RegisterModel<MultiVariateNormalModel>("General");

            // and store the factor
            _factor = model.AddFactor(Identifier, graph);
        }

        public override NArray Prepare(Context context)
        {
            return context.Factory.CreateNArray(context.Settings.SimulationCount, 1);
        }

        public override NArray Step(TimeInterval timeStep, NArray previous)
        {
            return _factor;
        }

        private NArray _factor;
    }
}
