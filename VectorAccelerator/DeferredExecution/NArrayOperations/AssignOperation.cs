using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator.DeferredExecution
{
    public class AssignOperation<T> : NArrayOperation<T>
    {
        public readonly NArray<T> Left;
        public readonly NArray<T> Right;

        public AssignOperation(NArray<T> left, NArray<T> right)
        {
            Left = left;
            Right = right;
        }

        public override bool IsVectorOperation
        {
            get { return true; }
        }

        public override string ToString()
        {
            return string.Join(" ", Left.ToString(), "=", Right.ToString());
        }
    }
}
