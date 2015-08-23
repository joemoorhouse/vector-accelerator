using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManagedCuda;

namespace VectorAcceleration.CUDA
{
    public class CUDAStorage
    {
        CudaDeviceVariable<float> _values;
    }
}
