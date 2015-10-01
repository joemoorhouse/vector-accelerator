using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator.Distributions
{
    public enum RandomNumberGeneratorType { MRG32K3A = 3145728 };

    public interface IRandomNumberGenerator 
    {
        void FillNormals(double[] toFill, RandomNumberStream stream);
    }
}
