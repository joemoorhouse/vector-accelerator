using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManagedCuda.CudaRand;
using ManagedCuda;
using ManagedCuda.NVRTC;
using ManagedCuda.CudaBlas;
using VectorAccelerator.NArrayStorage;
using VectorAccelerator.Distributions;

namespace VectorAccelerator.LinearAlgebraProviders.CUDA
{
    public class CUDALinearAlgebraProvider
    {
        CudaBlas _blas;
        
        public CUDALinearAlgebraProvider()
        {
            _blas = new CudaBlas();
        }

        public IDisposable CreateRandomNumberStream(RandomNumberGeneratorType type, int seed)
        {
            GeneratorType generatorType;
            switch (type)
            {
                case RandomNumberGeneratorType.MRG32K3A:
                    generatorType = GeneratorType.PseudoMRG32K3A;
                    break;
                default:
                    generatorType = GeneratorType.PseudoMRG32K3A;
                    break;
            }
            var device = new CudaRandDevice(generatorType);
            device.SetPseudoRandomGeneratorSeed((uint)seed);
            return device;
        }

        public void MatrixMultiply(NArray a, NArray b, NArray c)
        {
            //if (a.ColumnCount != a.RowCount)
            var dev_a = GetDeviceStorage<double>(a);
            var dev_b = GetDeviceStorage<double>(b);
            var dev_c = GetDeviceStorage<double>(c);
            _blas.Gemm(Operation.NonTranspose, Operation.NonTranspose, 
                a.RowCount, b.ColumnCount, a.ColumnCount, 1.0,
                        dev_a.Array, dev_a.Stride, dev_b.Array, dev_b.Stride, 
                        0.0, dev_c.Array, dev_c.Stride);
        }

        private DeviceStorage<T> GetDeviceStorage<T>(NArray<T> vector) where T : struct
        {
            return vector.Storage as DeviceStorage<T>;
        }
    }
}
