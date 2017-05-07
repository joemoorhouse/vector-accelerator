using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorAccelerator.Tests.Maths
{
    class PseudoInverseHelper
    {
        // b_f = A x
        // 128  =  128 x 3 3 x 1
        // x = A+ b
        // b_f = A A+ b 
        //
        // want dy  

        //lapack_int LAPACKE_dgels (int matrix_layout, char trans, lapack_int m, lapack_int n, lapack_int nrhs, double* a, lapack_int lda, double* b, lapack_int ldb);

        //\frac {\mathrm {d} }{\mathrm {d} x}}A^{+}(x)=-A^{+}\left({\frac {\mathrm {d} }{\mathrm {d} x}}A\right)A^{+}~+~A^{+}A^{+{\text{T}}}\left({\frac {\mathrm {d} }{\mathrm {d} x}}A^{\text{T}}\right)\left(I-AA^{+}\right)~+~\left(I-A^{+}A\right)\left({\frac {\text{d}}{{\text{d}}x}}A^{\text{T}}\right)A^{+{\text{T}}}A^{+}

        // dA/dr x + A dx/dr
        // need dx/dr = dA+/dr b + A+ db/dr
        //  

    }
}
