using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NArray.DeferredExecution;

namespace NArray.Interfaces
{
    public interface INArrayFactory
    {
        NArray NewNArray(int rows, int columns, double value);

        NArray NewNArrayLike(NArray other);

        NArray NewScalarNArray(double scalarValue);

        LocalNArray NewLocalNArray(int index, int rows, int columns, double value);

        LocalNArray NewLocalNArrayLike(int index, NArray other);

        LocalNArray NewLocalScalarNArray(int index, double scalarValue);
    }
}
