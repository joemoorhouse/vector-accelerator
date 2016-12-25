using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NArray;

namespace NArray.DeferredExecution
{
    public class LocalNArray : NArray
    {
        public int Index { get; set; }

        public LocalNArray(int rows, int columns) : base(rows, columns, StorageType.None) { }
    }
}
