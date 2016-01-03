using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskEngine.Framework
{
    public class IdentifierTypeKey : IEquatable<IdentifierTypeKey>
    {
        public readonly static char Delimiter = ':';

        public string Identifier { get; protected set; }

        public Type Type { get; protected set; }

        public IdentifierTypeKey(string identifier, Type type)
        {
            this.Identifier = identifier;
            this.Type = type;
        }

        #region IEquatable

        public bool Equals(IdentifierTypeKey other)
        {
            return Type == other.Type &&
                Identifier == other.Identifier;
        }

        public override bool Equals(object obj)
        {
            return obj != null && typeof(IdentifierTypeKey).IsAssignableFrom(obj.GetType()) &&
                Equals((IdentifierTypeKey)obj);
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode() ^ Identifier.GetHashCode();
        }

        #endregion

        public override string ToString()
        {
            return Type.Name + Delimiter + Identifier;
        }
    }
}
