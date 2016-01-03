using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskEngine.Framework
{
    public struct NodeGroupKey
    {
        public Type Type;
        public int TreeLevel;

        public NodeGroupKey(Type type, int treeLevel)
        {
            Type = type;
            TreeLevel = treeLevel;
        }
    }
}
