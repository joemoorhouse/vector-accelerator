using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskEngine.Framework
{
    public class SimulationSet
    {
        public readonly SimulatingModel_OLD[] Models;

        public readonly Type Type;

        public SimulationSet(IEnumerable<SimulationNode> nodes)
        {
            Type = nodes.First().Key.Type;
            if (nodes.Where(n => n.Key.Type != Type).Any())
                throw new Exception("ISimulation types must all be the same.");
            Models = nodes.Select(n => n.Model as SimulatingModel_OLD).ToArray();
        }
    }
}
