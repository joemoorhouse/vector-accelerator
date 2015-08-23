using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VectorAccelerator.LinearAlgebraProviders;

namespace VectorAccelerator.DeferredExecution
{
    class VectorOperationRunner
    {
        public static void Execute(ExpressionBuilder builder, 
            IImmediateLinearAlgebraProvider provider)
        {

            foreach (var operation in builder.VectorOperations)
            {
                if (operation is BinaryVectorOperation)
                {
                    var binaryVectorOperation = operation as BinaryVectorOperation;
                    
                }
            }
        }

        private void AllocateStorageToLocalNArray(LocalNArray localNArray, int chunk)
        {

        }

    }
}
