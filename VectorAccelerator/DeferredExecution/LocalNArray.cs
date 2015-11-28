using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorAccelerator
{
    public interface ILocalNArray
    {
        int Index { get; }
    }
    
    /// <summary>
    /// NArray that represents local storage (i.e. not persisted outside scope)
    /// </summary>
    public class LocalNArray : NArray, ILocalNArray
    {
        int _index;

        public LocalNArray(int index, int length) : base(StorageLocation.None, length)
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

    /// <summary>
    /// NArray that represents local storage (i.e. not persisted outside scope)
    /// </summary>
    public class LocalNArrayInt : NArrayInt, ILocalNArray
    {
        int _index;

        public LocalNArrayInt(int index, int length)
            : base(StorageLocation.None, length)
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
