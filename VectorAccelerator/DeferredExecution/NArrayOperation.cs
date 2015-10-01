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
        public NArray Result;

        /// <summary>
        /// Whether this operation is a vector operation (i.e. can be split into smaller vector operations)
        /// </summary>
        public virtual bool IsVectorOperation { get { return false; } }

        public virtual NArrayOperation Clone(Func<NArray, NArray> transform)
        {
            return new NArrayOperation() { Result = transform(Result) };
        }

        public virtual IList<NArray> Operands()
        {
            return null;
        }
    }
}
