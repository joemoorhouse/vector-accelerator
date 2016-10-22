using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskEngine.Models;
using System.Reflection;
using RiskEngine.Factors;
using VectorAccelerator;

namespace RiskEngine.Framework
{    
    public class SimulationGraph
    {
        public SimulationGraph(StorageLocation location, DateTime closeOfBusinessDate)
        {
            Context = new Context(location, closeOfBusinessDate);
            var rootNodeKey = new ModelKey("Root", typeof(DummyModel));
            var rootNode = new SimulationNode(rootNodeKey);
            rootNode.Model = Model.Create<DummyModel>("Root", null);
            _nodes[rootNodeKey] = rootNode;
            _nodeStack.Push(rootNode);
        }

        /// <summary>
        /// Provides data required to populate the graph
        /// </summary>
        public readonly Context Context;

        public T RegisterFactor<T>(string identifier) where T : Factor
        {
            // if the factor already exists, then return it
            var factorKey = new FactorKey(identifier, typeof(T));
            SimulationNode node;
            ModelKey modelKey;
            if (!_modelLookup.TryGetValue(factorKey, out modelKey))
            {
                // otherwise create. Note that creating the factor always creates the corresponding model
                var factor = _factory.CreateFactor(typeof(T), identifier, this) as T;
                var model = factor.Model;
                modelKey = new ModelKey(identifier, model.GetType());
                node = AddModelToGraph(modelKey, model);
                node.Factor = factor;
                _modelLookup[factorKey] = modelKey;
            }
            else
            {
                node = _nodes[modelKey];
            }
            _nodeStack.Peek().AddRequisite(node);
            return (T)node.Factor;
        }

        public T RegisterModel<T>(string identifier) where T : Model
        {
            var modelKey = new ModelKey(identifier, typeof(T));
            SimulationNode node;
            if (!_nodes.TryGetValue(modelKey, out node))
            {
                var model = _factory.CreateModel(typeof(T), identifier, this) as T;
                node = AddModelToGraph(modelKey, model);
            }
            _nodeStack.Peek().AddRequisite(node);
            return (T)node.Model;        }

        private SimulationNode AddModelToGraph(ModelKey key, Model model)
        {
            var node = new SimulationNode(key) { Model = model };
            _nodes[key] = node;
            _nodeStack.Push(node);
            // add children here
            model.Initialise(this);
            _nodeStack.Pop();
            return node;
        }

        public SimulationRunner ToSimulationRunner()
        {
            var simulatingNodes = _nodes.Values.Where(n => typeof(IEvolvingModel).IsAssignableFrom(n.Key.Type));

            var orderedNodes = simulatingNodes.
                GroupBy(n => new NodeGroupKey(n.Key.Type, n.GetTreeLevel())).
                    OrderBy(k => k.Key.TreeLevel);

            return new SimulationRunner(orderedNodes.Select(g => new SimulationSet(g)), 
                _nodes.Values.Select(n => n.Model), Context);
        }

        Dictionary<FactorKey, ModelKey> _modelLookup = new Dictionary<FactorKey, ModelKey>();
        Dictionary<ModelKey, SimulationNode> _nodes = new Dictionary<ModelKey, SimulationNode>();
        Stack<SimulationNode> _nodeStack = new Stack<SimulationNode>();
        Factory _factory = new Factory();
    }
}
