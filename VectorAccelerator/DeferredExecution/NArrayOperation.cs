using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using VectorAccelerator.LinearAlgebraProviders;

namespace VectorAccelerator.DeferredExecution
{
    public class NArrayOperation
    {
        /// <summary>
        /// Whether this operation is a vector operation (i.e. can be split into smaller vector operations)
        /// </summary>
        public virtual bool IsVectorOperation { get { return false; } }
    }

    public class NArrayOperation<T> : NArrayOperation
    {
        public NArray<T> Result;

        public virtual NArrayOperation<T> Clone(Func<NArray<T>, NArray<T>> transform)
        {
            return new NArrayOperation<T>() { Result = transform(Result) };
        }

        public virtual IList<NArray<T>> Operands()
        {
            return null;
        }
    }
}
