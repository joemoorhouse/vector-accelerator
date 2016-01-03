using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskEngine.Framework
{
    public class ModelKey : IdentifierTypeKey
    {
        public ModelKey(string identifier, Type type) : base(identifier, type) { }

        public bool Matches(Model model)
        {
            return model.GetType() == this.Type;
        }
    }
}
