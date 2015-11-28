using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator;

namespace RiskEngine.Framework
{
    public class ModelFactory
    {
        public T CreateDefaultModel<T>(string identifier) where T : IFactor
        {
            return default(T);
        }
        
        public T CreateModelOfType<T>(string identifier) where T : IFactor
        {
            return default(T);
        }
    }
}
