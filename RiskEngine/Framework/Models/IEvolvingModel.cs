using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskEngine.Framework
{
    /// <summary>
    /// Implemented by a Model that can evolve itself to the next simulation time point.
    /// </summary>
    public interface IEvolvingModel
    {
        void StepNext(TimeInterval interval);
    }
}
