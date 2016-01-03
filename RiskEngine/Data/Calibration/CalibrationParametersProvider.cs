using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskEngine.Framework;

namespace RiskEngine.Data
{
    public class ParameterProvider<T>
    {
        public virtual T Value(string factor)
        {
            return _cache[factor];
        }

        public virtual void AddValue(string factor, T correlation)
        {
            _cache[factor] = correlation;
        }

        protected Dictionary<string, T> _cache = new Dictionary<string, T>(); 
    }

    public class TwoFactorParameterProvider<T>
    {
        public virtual T Value(string factor1, string factor2)
        {
            return _cache[new Tuple<string, string>(factor1, factor2)];
        }

        public virtual void AddValue(string factor1, string factor2, T correlation)
        {
            _cache[new Tuple<string, string>(factor1, factor2)] = correlation;
        }

        protected Dictionary<Tuple<string, string>, T> _cache = new Dictionary<Tuple<string, string>, T>(); 
    }

    public enum DataItem { Correlation, Volatility };
}
