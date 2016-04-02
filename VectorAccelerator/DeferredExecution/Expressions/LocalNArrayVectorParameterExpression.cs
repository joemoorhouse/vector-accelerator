using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorAccelerator.DeferredExecution.Expressions
{
    public class LocalNArrayVectorParameterExpression<T> : ReferencingVectorParameterExpression<T>
    {
        public LocalNArrayVectorParameterExpression(NArray<T> array)
            : base(array, ParameterType.Local, (array as ILocalNArray).Index) { }
    }
}
