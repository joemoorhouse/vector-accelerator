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
        int _length;

        public LocalNArray(int index, int length) 
        {
            _index = index;
            _length = length;
        }

        public override NArrayStorage<double> Storage
        {
            get
            {
                return base.Storage;
            }
            set
            {
                base.Storage = value;
                _length = Storage.Length;
            }
        }

        public int Index
        {
            get { return _index; }
        }

        public override int Length
        {
            get { return _length; }
        }

        public override string ToString()
        {
            return string.Format("Local{0}", _index);
        }
    }
}
