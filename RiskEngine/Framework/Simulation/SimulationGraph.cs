using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskEngine.Models;
using System.Reflection;

namespace RiskEngine.Framework
{    
    public class SimulationGraph
    {
        public SimulationGraph()
        {
            _defaults = new Dictionary<Type, Type>()
            {
                { typeof(INormalVariates), typeof(NormalVariatesModel) }
            };
            var rootNodeKey = new ModelKey("Root", typeof(DummyModel));
            var rootNode = new SimulationNode(rootNodeKey);
            rootNode.Model = Model.Create<DummyModel>("Root", null);
            _nodes[rootNodeKey] = rootNode;
            _nodeStack.Push(rootNode);
        }

        public T DefaultModel<T>(string identifier, Simulation simulation) where T : class
        {
            var modelType = _defaults[typeof(T)];
            // if the model is a MultiFactor model, then we get the model from the MultiFactor model
            return GetModel<T>(modelType, identifier, simulation);
        }

        public T ModelOfType<T>(string identifier, Simulation simulation) where T : class
        {
            return GetModel<T>(typeof(T), identifier, simulation);
        }

        public SimulationRunner ToSimulationRunner(Context context)
        {
            var simulatingNodes = _nodes.Values.Where(n => typeof(SimulatingModel_OLD).IsAssignableFrom(n.Key.Type));

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
                    MethodInfo genericMethod = typeof(Model).GetMethod("Create").MakeGenericMethod( modelType);
                    var creationDelegate = Delegate.CreateDelegate(typeof(CreateModel), genericMethod) as CreateModel;
                    // this delegate can be created once and then cached for good performance (this should then perform about as 
                    // well as calling new)
                    var model = creationDelegate(identifier, simulation);
                    node.Model = model;
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

        Dictionary<Type, Type> _defaults;
        Dictionary<ModelKey, SimulationNode> _nodes = new Dictionary<ModelKey,SimulationNode>();
        Stack<SimulationNode> _nodeStack = new Stack<SimulationNode>();
    }
}
