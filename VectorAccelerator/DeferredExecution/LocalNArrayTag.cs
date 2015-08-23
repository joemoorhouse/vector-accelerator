using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorAccelerator
{
    /// <summary>
    /// The tag refers to a temporary NArray (i.e. one that is temporarily created within the context)
    /// </summary>
    public class LocalNArrayTag 
    {        
        public int Index { get; private set; }

        public LocalNArrayTag(int index)
        {
            Index = index;
        }
    }
}
