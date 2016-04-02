using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskEngine.Models;
using System.Reflection;
using RiskEngine.Factors;

namespace RiskEngine.Framework
{    
    public class SimulationGraph
    {
        public SimulationGraph()
        {
            var rootNodeKey = new ModelKey("Root", typeof(DummyModel));
            var rootNode = new SimulationNode(rootNodeKey);
            rootNode.Model = Model.Create<DummyModel>("Root", null);
            _nodes[rootNodeKey] = rootNode;
            _nodeStack.Push(rootNode);
        }

        public T GetFactor<T>(string identifier, Simulation simulation) where T : class
        {
            return _factory.CreateFactor(typeof(T), identifier, simulation) as T;
        }

        public T GetModel<T>(string identifier, Simulation simulation) where T : class
        {
            return _factory.CreateModel(typeof(T), identifier, simulation) as T;
        }

        public SimulationRunner ToSimulationRunner(Context context)
        {
            var simulatingNodes = _nodes.Values.Where(n => typeof(EvolvingModel).IsAssignableFrom(n.Key.Type));

            var orderedNodes = simulatingNodes.
                GroupBy(n => new NodeGroupKey(n.Key.Type, n.GetTreeLevel())).
                    OrderBy(k => k.Key.TreeLevel);

            return new SimulationRunner(orderedNodes.Select(g => new SimulationSet(g)), context);
        }

        public delegate Model CreateModel(string identifier, Simulation simulation); 

        private T GetModel<T>(Type modelType, string identifier, Simulation simulation) where T : class
        {
            var key = new ModelKey(identifier, modelType);
            SimulationNode node = null;
            if (!_nodes.TryGetValue(key, out node))
            {
                node = new SimulationNode(key);
                _nodeStack.Peek().AddRequisite(node);
                _nodeStack.Push(node);
                try
                {
                    node.Model = _factory.CreateModel(modelType, identifier, simulation);
                }
                catch (Exception)
                {
                    throw new Exception("Model constructor not available");
                }
                _nodes[key] = node;
                _nodeStack.Pop();
            }
            else _nodeStack.Peek().AddRequisite(node);
            return node.Model as T;
        }

        Dictionary<FactorKey, SimulationNode> _factors = new Dictionary<FactorKey, SimulationNode>();
        Dictionary<ModelKey, SimulationNode> _nodes = new Dictionary<ModelKey, SimulationNode>();
        Stack<SimulationNode> _nodeStack = new Stack<SimulationNode>();
        Factory _factory = new Factory();
    }
}
