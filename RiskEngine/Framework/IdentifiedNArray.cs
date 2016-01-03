using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator;

namespace RiskEngine.Framework
{
    /// <summary>
    /// Utility class that contains an NArray and its identifier 
    /// </summary>
    public class IdentifiedNArray 
    {
        public NArray Value { get { return _value; } set { _value = value; } }

        public readonly string Identifier;

        public IdentifiedNArray(string identifier)
        {
            Identifier = identifier;
        }

        private NArray _value;
    }
}
