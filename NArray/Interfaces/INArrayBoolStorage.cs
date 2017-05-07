using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NArray.Interfaces
{
    public interface INArrayBoolStorage : INArrayShape
    {
        bool[] Data { get; set; }

        int DataStart { get; set; }

        bool this[params int[] indices] { get; set; }
    }
}
