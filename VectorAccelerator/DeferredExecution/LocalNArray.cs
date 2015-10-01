using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorAccelerator
{
    /// <summary>
    /// NArray that represents local storage (i.e. not persisted outside scope)
    /// </summary>
    public class LocalNArray : NArray
    {
        int _index;

        public LocalNArray(int index, int length) : base(length)
        {
            _index = index;
        }

        public int Index
        {
            get { return _index; }
        }

        public override string ToString()
        {
            return string.Format("Local{0}", _index);
        }
    }
}
