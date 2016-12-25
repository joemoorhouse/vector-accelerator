using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorAccelerator
{
    public interface INArray { }

    public interface INArray<T> : INArray
    {
        bool IsScalar { get; }
        bool IsVector { get; }
        bool IsMatrix { get; }

        int Length { get; }

        T First();

        INArrayStorage<T> Storage { get; set; }

        NArray<T> Slice(int startIndex, int length);
    }
}
