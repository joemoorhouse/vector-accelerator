using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskEngine.Framework;

namespace RiskEngine.Factors
{
    public class FactorKey
    {
        Type _type;
        string _identifier;

        private FactorKey(Type type, string identifier)
        {
            _type = type;
            _identifier = identifier;
        }

        public static FactorKey New<T>(string identifier) where T : IFactor
        {
            return new FactorKey(typeof(T), identifier);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_type.GetHashCode() * 397) ^ _identifier.GetHashCode();
            }
        }
    }

    public class FactorPairKey : IEquatable<FactorPairKey>
    {
        FactorKey _factor1;
        FactorKey _factor2;

        public FactorPairKey(FactorKey factor1, FactorKey factor2)
        {
            _factor1 = factor1;
            _factor2 = factor2;
        }

        public bool Equals(FactorPairKey other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return string.Equals(_factor1, other._factor1) && string.Equals(_factor2, other._factor2);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((FactorPairKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_factor1 != null ? _factor1.GetHashCode() : 0) * 397) ^ (_factor2 != null ? _factor2.GetHashCode() : 0);
            }
        }
    }
    
}
