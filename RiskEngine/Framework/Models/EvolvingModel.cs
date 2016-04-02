using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskEngine.Framework
{
    /// <summary>
    /// A Model that can evolve itself to the next simulation time point.
    /// </summary>
    public abstract class EvolvingModel : Model
    {
        public abstract void StepNext();

        public abstract void Prepare(Context context);
    }
}
