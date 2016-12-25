using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NArray
{
    /// <summary>
    /// The cut-down part of AbstractNArray upon which LinearAlgebraProviders act
    /// </summary>
    public interface INArrayStorage : INArrayShape
    {
        double[] Data { get; set; }

        int DataStart { get; set; }

        double this[params int[] indices] { get; set; }
    }
}
