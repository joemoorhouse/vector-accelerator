using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskEngine.Framework
{
    public class Pricer<T> where T : new()
    {
        protected T _deal = new T();

        public T Deal { get { return _deal; } }
    }
}
