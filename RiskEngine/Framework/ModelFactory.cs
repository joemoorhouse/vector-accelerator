using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using VectorAccelerator;
using RiskEngine.Models;

namespace RiskEngine.Framework
{
    public class ModelFactory
    {
        public ModelFactory()
        {
            _defaults = new Dictionary<Type, Type>()
            {
                { typeof(INormalVariates), typeof(NormalVariatesModel) }
            };
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

        private T GetModel<T>(Type modelType, string identifier, Simulation simulation) where T : class
        {
            var key = new ModelKey(identifier, modelType);
            Model model = null;
            if (!_models.TryGetValue(key, out model))
            {
                try
                {
                    model = Activator.CreateInstance(modelType, new object[] { identifier, simulation }) as Model;
                }
                catch (Exception)
                {
                    throw new Exception("Model constructor not available");
                }
            }
            return model as T;
        }

        Dictionary<Type, Type> _defaults;
        Dictionary<ModelKey, Model> _models;
    }
}
