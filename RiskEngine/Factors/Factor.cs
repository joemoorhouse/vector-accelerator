using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator;

namespace RiskEngine.Framework
{
    public class Factor : IFactor
    {
        public NArray Value { get { return _value; } set { _value = value; } }

        public readonly string Identifier;

        public Factor(MultiFactorModel model, string identifier)
        {
            _model = model; Identifier = identifier;
        }

        private MultiFactorModel _model;
        private NArray _value;
    }
}
