using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator;

namespace RiskEngine.Framework
{    
    public class Factor
    {
        protected Model _model;

        public static U Create<U, V>(string identifier, Simulation simulation)
            where U : Factor, new()
            where V : Model, new()
        {
            var model = RiskEngine.Framework.Model.Create<V>(identifier, simulation);
            var factor = new U();
            factor._model = model;
            //factor.Identifier = identifier;
            return factor;
        }
    }
    
    /// <summary>
    /// Factor that holds a reference to the generating model.
    /// This can used be used to calculate the factor values on request, or can be used to expose Model parameters.
    /// </summary>
    /// <typeparam name="T">Interface implemented by the generating model.</typeparam>
    public class Factor<T> : Factor where T : class
    {
        public T Model { get { return _model as T; } }
    }
}
