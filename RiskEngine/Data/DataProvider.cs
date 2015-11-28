using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskEngine.Data
{
    public class DataProvider
    {
        public T CalibrationParametersProviderOfType<T>() where T : class
        {
            var provider = _providers[typeof(T)] as T;
            return provider;
        }

        Dictionary<Type, object> _providers;
    }
}
