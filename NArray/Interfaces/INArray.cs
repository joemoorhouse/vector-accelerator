using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NArray
{
    public interface INArray : INArrayShape
    {
        INArrayStorage Storage { get; set; }

        double this[params int[] indices] { get; set; }
    }
}
