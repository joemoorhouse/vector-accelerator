using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NArray;
using NArray.Interfaces;

namespace NArray.DeferredExecution
{
    public class LocalNArray : NArray, ILocalNArray
    {
        public int Index { get; set; }

        public LocalNArray(int rows, int columns) : base(rows, columns, StorageType.None) { }
    }
}
