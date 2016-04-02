using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace RiskEngine.Framework
{
    public class ModelAppliesToAttribute : Attribute
    {
        public Type[] FactorTypes { get { return _factorTypes; } }

        public ModelAppliesToAttribute(params Type[] factorTypes)
        {
            _factorTypes = factorTypes;
        }

        Type[] _factorTypes;
    }
}
