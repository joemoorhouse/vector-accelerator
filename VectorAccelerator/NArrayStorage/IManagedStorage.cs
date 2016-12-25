using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorAccelerator.NArrayStorage
{
    /// <summary>
    /// Storage that makes use of a contiguous block of managed memory.
    /// The storage can be offset from the start of the underlying data block.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    interface IManagedStorage<T> : INArrayStorage<T>
    {
        T[] Data { get; }

        int DataStartIndex { get; }

        //double this[params int[] indices] { get; set; }
    }
}
