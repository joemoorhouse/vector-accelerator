using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NArray
{
    /// <summary>
    /// Defines the shape (i.e. dimensions) of an NArray (and is host/device agnostic)
    /// </summary>
    public interface INArrayShape
    {
        bool IsTranspose { get; }

        bool IsScalar { get; }

        bool IsArray { get; }

        bool IsMatrix { get; }

        int TotalSize { get; }

        int Rows { get; }

        int Columns { get; }
    }
}
