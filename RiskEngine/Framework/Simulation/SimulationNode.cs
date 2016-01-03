using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskEngine.Framework
{
    public class SimulationNode
    {
        /// <summary>
        /// Child nodes that node requires to be simulated before it can be simulated.
        /// </summary>
        public readonly List<SimulationNode> Requisites = new List<SimulationNode>();
        
        /// <summary>
        /// The instance of the Model
        /// </summary>
        public Model Model
        {
            get { return _model; }
            set
            {
                if (!Key.Matches(value)) throw new Exception("Model does not match Key");
                _model = value;
            }
        }

        /// <summary>
        /// The Model key
        /// </summary>
        public ModelKey Key { get; private set; }

        public SimulationNode(ModelKey key)
        {
            this.Key = key;
        }

        /// <summary>
        /// State that this node requires the specified node to be simulated before it itself can be
        /// simulated.
        /// </summary>
        /// <param name="node"></param>
        public void AddRequisite(SimulationNode node)
        {
            // if node requires this node, then there is a circular dependence
            if (node.Requires(this))
                throw new ArgumentException("leads to circular reference");

            foreach (var existingRequisite in Requisites)
            {
                if (node.Key == existingRequisite.Key)
                    return;
            }
            Requisites.Add(node);
        }

        public bool Requires(SimulationNode node)
        {
            foreach (var requisite in Requisites)
                if ((requisite == node) || requisite.Requires(node)) return true;
            return false;
        }

        public int GetTreeLevel()
        {
            int maxRequisiteLevel = 0;
            foreach (var node in Requisites)
            {
                int requisiteLevel = node.GetTreeLevel();
                maxRequisiteLevel = Math.Max(maxRequisiteLevel, requisiteLevel);
            }
            return maxRequisiteLevel + 1;
        }

        public override string ToString()
        {
            return _model.ToString();
        }

        Model _model;
    }
}
