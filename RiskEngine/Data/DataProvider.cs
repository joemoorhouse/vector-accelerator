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

        public T AddCalibrationParametersProvider<T>(T newProvider) where T : class
        {
            _providers[typeof(T)] = newProvider;
            return newProvider;
        }

        Dictionary<Type, object> _providers = new Dictionary<Type,object>();
    }
}
