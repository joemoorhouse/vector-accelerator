using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator;
using RiskEngine.Framework;
using RiskEngine.Models;

namespace RiskEngine.Calibration
{
    public class CorrelationHelper
    {
        public static void AddMultivariateModelWeightsProvider(Context context,
            IList<string> identifiers, NArray correlationMatrix)
        {
            correlationMatrix = CorrelationHelper.NearestCorrelationMatrix(correlationMatrix);
            
            var weightsCount = correlationMatrix.Rows;
            var weightsMatrix = NMath.CholeskyDecomposition(correlationMatrix).Transpose();

            var weights = context.Data.AddCalibrationParametersProvider
                (new WeightsProvider(weightsCount));

            for (int i = 0; i < weightsMatrix.Columns; ++i)
            {
                weights.AddValue(identifiers[i], weightsMatrix.GetColumn(i));
            }
        }
        
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
            var flooredDiagonal = NMath.Diagonal(NMath.Sqrt(eigenvalues));
            var root = eigenvectors * flooredDiagonal;

            // next we normalise the rows
            for (int i = 0; i < root.Rows; ++i)
            {
                var row = root.GetRow(i);
                root.SetRow(i, row / NMath.Sqrt(row * row.Transpose()));
            }

            var nearestCorrelation = root * root.Transpose();

            return nearestCorrelation;
        }
    }
}
