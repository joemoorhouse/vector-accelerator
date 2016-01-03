using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskEngine.Framework;
using VectorAccelerator;

namespace RiskEngine.Models
{
    public class NormalVariatesModel : Model, INormalVariates
    {
        public NArray Value { get { return _factor.Value; } }

        public override void Initialise(Simulation simulation)
        {
            // we get the (single) instance of the MultiVariateNormalModel
            var model = simulation.ModelOfType<MultiVariateNormalModel>("General");
            
            // and store the factor
            _factor = model.AddFactor(Identifier);
        }

        private IdentifiedNArray _factor;
    }
}
