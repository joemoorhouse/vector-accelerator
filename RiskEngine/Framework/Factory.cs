using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

namespace RiskEngine.Framework
{
    public class Factory
    {
        public delegate Model CreateNewModel(string identifier, Simulation simulation);

        public delegate object CreateNewFactor(string identifier, Simulation simulation);

        public Factory()
        {
            _factorModels = GetFactorModels();
        }

        public Model CreateModel(Type modelType, string identifier, Simulation simulation)
        {
            MethodInfo genericMethod = typeof(Model).GetMethod("Create").MakeGenericMethod(modelType);
            var creationDelegate = Delegate.CreateDelegate(typeof(CreateNewModel), genericMethod) as CreateNewModel;
            var model = creationDelegate(identifier, simulation);
            return model as Model;
        }

        public object CreateFactor(Type factorType, string identifier, Simulation simulation) 
        {
            var modelType = _factorModels[factorType].First();
            MethodInfo genericMethod = typeof(Factor).GetMethod("Create").MakeGenericMethod(factorType, modelType);
            var creationDelegate = Delegate.CreateDelegate(typeof(CreateNewFactor), genericMethod) as CreateNewFactor;
            var model = creationDelegate(identifier, simulation);
            return model;
        }

        private Dictionary<Type, List<Type>> GetFactorModels()
        {
            var lookup = new Dictionary<Type, List<Type>>();

            var allModels = typeof(Model).Assembly.GetTypes().
                Where(t => typeof(Model).IsAssignableFrom(t));

            foreach (var model in allModels)
            {
                var attributes = Attribute.GetCustomAttributes(model).OfType<ModelAppliesToAttribute>();
                var factors = attributes.SelectMany(a => a.FactorTypes);
                foreach (var factor in factors)
                {
                    if (!lookup.ContainsKey(factor))
                    {
                        lookup[factor] = new List<Type>();
                    }
                    lookup[factor].Add(model);
                }
            }
            return lookup;
        }

        Dictionary<Type, List<Type>> _factorModels = new Dictionary<Type, List<Type>>(); // looks up the posible Model types for a given Factor type
    }
}
