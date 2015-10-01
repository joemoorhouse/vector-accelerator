using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator.DeferredExecution
{
    public class AssignOperation : NArrayOperation
    {
        public readonly NArray Left;
        public readonly NArray Right;

        public AssignOperation(NArray left, NArray right)
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
