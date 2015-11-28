using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator;
using VectorAccelerator.Distributions;

namespace VectorAccelerator.Tests
{
    public class LinearAlgebraTests
    {
        public void IntelMKLTests()
        {
            var factory = new NArrayFactory(StorageLocation.Host);

            using (var random = factory.CreateRandom())
            {
                var normal = new Normal(random, 0, 1);
                
                //var a = factory.CreateNArray(4, 4);
                //a.FillRandom(normal);

                var a = factory.CreateNArray(new double[,] {
                    { 1.1,  4.2,    -2.3},
                    { 4.2,  5,      1.3},
                    { -2.3, 1.3,    7}
                });

                NArray eigenvalues, eigenvectors;
                NMath.EigenvalueDecomposition(a, out eigenvectors, out eigenvalues);

                var diagonal = NMath.Diagonal(eigenvalues);
                var result = eigenvectors * diagonal * eigenvectors.Transpose();


                var correlation = factory.CreateNArray(new double[,] {
                    { 1.0,  0.89,   -0.52},
                    { 0.89, 1.0,    0.12},
                    { -0.52, 0.12,   1.0}
                });
                NMath.EigenvalueDecomposition(a, out eigenvectors, out eigenvalues);

                // we multiply the columns of eigenvectors by the floored eigenvalues
                var floorValue = 1e-6;
                eigenvalues[eigenvalues < floorValue] = floorValue;
                var flooredDiagonal = NMath.Diagonal(eigenvalues);
                var root = eigenvectors * flooredDiagonal;

                // next we normalise the rows
                for (int i = 0; i < root.RowCount; ++i)
                {
                    var row = root.Row(i);
                    root.SetRow(i, row / NMath.Sqrt(row * row.Transpose()));
                }

                var nearestCorrelation = root * root.Transpose();

                var test = NMath.CholeskyDecomposition(nearestCorrelation);

                Console.WriteLine(nearestCorrelation.ToString());
            }
        }
    }
}
