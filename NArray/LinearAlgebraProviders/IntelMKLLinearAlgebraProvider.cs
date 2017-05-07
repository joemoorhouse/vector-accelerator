using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NArray.Interfaces;
using NArray.DeferredExecution.Expressions;
using NArray.LinearAlgebraProviders;

namespace NArray
{
    class IntelMKLLinearAlgebraProvider : ILinearAlgebraProvider
    {
        // where no MKL acceleration is available, we revert to managed
        ILinearAlgebraProvider _managedProvider = new ManagedLinearAlgebraProvider();

        public void BinaryElementWiseOperation(INArrayStorage left, INArrayStorage right, ExpressionType operation, INArrayStorage result)
        {
            VectorVectorOperation vectorVectorOperation = null;
            switch (operation)
            {
                case ExpressionType.Add: vectorVectorOperation = IntelVectorMathLibraryWrapper.Add; break;
                case ExpressionType.Subtract: vectorVectorOperation = IntelVectorMathLibraryWrapper.Subtract; break;
                case ExpressionType.Multiply: vectorVectorOperation = IntelVectorMathLibraryWrapper.Multiply; break;
                case ExpressionType.Divide: vectorVectorOperation = IntelVectorMathLibraryWrapper.Divide; break;
            }
            VectorVectorOperation(left, right, result, vectorVectorOperation);
        }

        public void UnaryElementWiseOperation(INArrayStorage operand,
            INArrayStorage result, UnaryElementWiseOperations operation)
        {
            if (operation == UnaryElementWiseOperations.Negate)
            {
                ScaleOffset(operand, -1, 0, result);
                return;
            }
            VectorOperation vectorVectorOperation = null;
            switch (operation)
            {
                case UnaryElementWiseOperations.CumulativeNormal: vectorVectorOperation = IntelVectorMathLibraryWrapper.CumulativeNormal; break;
                case UnaryElementWiseOperations.Exp: vectorVectorOperation = IntelVectorMathLibraryWrapper.Exp; break;
                case UnaryElementWiseOperations.InverseCumulativeNormal: vectorVectorOperation = IntelVectorMathLibraryWrapper.InverseCumulativeNormal; break;
                case UnaryElementWiseOperations.InverseSquareRoot: vectorVectorOperation = IntelVectorMathLibraryWrapper.InverseSquareRoot; break;
                case UnaryElementWiseOperations.Inverse: vectorVectorOperation = IntelVectorMathLibraryWrapper.Inverse; break;
                case UnaryElementWiseOperations.Log: vectorVectorOperation = IntelVectorMathLibraryWrapper.Log; break;
                case UnaryElementWiseOperations.SquareRoot: vectorVectorOperation = IntelVectorMathLibraryWrapper.SquareRoot; break;
            }
            VectorOperation(operand, result, vectorVectorOperation);
        }

        public void ScaleInverse(INArrayStorage operand, double scale, INArrayStorage result)
        {
            VectorOperation(operand, result, IntelVectorMathLibraryWrapper.Inverse);
            IntelVectorMathLibraryWrapper.ConstantAddMultiply(operand.Data, 0, scale, 0, result.Data, 0, result.TotalSize);
        }

        public void ScaleOffset(INArrayStorage operand, double scale, double offset, INArrayStorage result)
        {
            IntelVectorMathLibraryWrapper.ConstantAddMultiply(operand.Data, 0, scale, offset, result.Data, 0, result.TotalSize);
        }

        private static void VectorVectorOperation(INArrayStorage left, INArrayStorage right, INArrayStorage result, VectorVectorOperation operation)
        {
            operation(left.Data, 0, right.Data, 0, result.Data, 0, result.TotalSize);
        }

        private static void VectorOperation(INArrayStorage operand, INArrayStorage result, VectorOperation operation)
        {
            operation(operand.Data, 0, result.Data, 0, result.TotalSize);
        }

        public void RelativeElementWiseOperation(INArrayStorage left, INArrayStorage right, ExpressionType operation, INArrayBoolStorage result)
        {
            _managedProvider.RelativeElementWiseOperation(left, right, operation, result);
        }

        public void RelativeElementWiseOperation(INArrayStorage left, double right, ExpressionType operation, INArrayBoolStorage result)
        {
            _managedProvider.RelativeElementWiseOperation(left, right, operation, result);
        }
    }

    delegate void VectorVectorOperation(double[] a, int aStartIndex,
        double[] b, int bStartIndex,
        double[] y, int yStartIndex,
        int length);

    delegate void VectorOperation(double[] a, int aStartIndex,
        double[] y, int yStartIndex,
        int length);
}
