using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator;

namespace RiskEngine.Calibration
{
    public class CorrelationHelper
    {
        public static NArray CorrelationMatrixToWeights(NArray correlationMatrix)
        {
            var nearestCorrelationMatrix = NearestCorrelationMatrix(correlationMatrix);
            var cholesky = NMath.CholeskyDecomposition(nearestCorrelationMatrix);

            // the weights are simply the Cholesky decomposition
            return cholesky;
        }

        public static NArray NearestCorrelationMatrix(NArray candidateMatrix)
        {
            NArray eigenvectors, eigenvalues;
            NMath.EigenvalueDecomposition(candidateMatrix, out eigenvectors, out eigenvalues);

            // we multiply the columns of eigenvectors by the floored eigenvalues
            var floorValue = 1e-9;
            eigenvalues[eigenvalues < floorValue] = floorValue;
            var flooredDiagonal = NMath.Diagonal(eigenvalues);
            var root = eigenvectors * flooredDiagonal;

            // next we normalise the rows
            for (int i = 0; i < root.RowCount; ++i)
            {
                var row = root.Row(i);
                root.SetRow(i, row / NMath.Sqrt(row * row.Transpose()));
            }

            return root * root.Transpose();
        }
    }
}
