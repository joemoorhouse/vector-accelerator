using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator
{
    public static class Exceptions
    {
        public static Exception OperationNotSupportedForReferenceMatrices()
        {
            return new ArgumentException("operation is not supported for reference matrices");
        }
    }
}
