using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace VectorAccelerator.DeferredExecution
{
    public class CPULibraryScheduler
    {
        ExpressionBuilder _builder;

        Stack<double[]> _temporaryVectorCache;

        public void Execute()
        {
            foreach (var expression in _builder.Expressions)
            {
                var binaryExpression = expression as BinaryExpression;
                if (binaryExpression != null)
                {

                }

            }
        }
    }
}
