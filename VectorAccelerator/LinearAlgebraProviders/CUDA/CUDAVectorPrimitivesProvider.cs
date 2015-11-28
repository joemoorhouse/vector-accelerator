using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManagedCuda;
using ManagedCuda.BasicTypes;
using ManagedCuda.VectorTypes;
using ManagedCuda.NVRTC;
using ManagedCuda.NPP;

namespace VectorAccelerator.LinearAlgebraProviders.CUDA
{
    public class CUDAVectorPrimitivesProvider
    {
        public void Compile()
        {
            using (var ctx = new CudaContext())
            {
                // with verbaim string @, we only have to double up double quotes: no other escaping
                string source = @"
                extern ""C"" __global__ 
                void saxpy(float a, float *x, float *y, float *out, size_t n)
                { 
	                size_t tid = blockIdx.x * blockDim.x + threadIdx.x; 
	                if (tid < n) 
	                { 
		                out[tid] = a * x[tid] + y[tid]; 
	                } 
                }
                ";

                string source2 = @"
                extern ""C"" __global__ 
                void saxpy(float a, float *x, float *y, float *out, size_t n)
                { 
                    size_t tid = blockIdx.x * blockDim.x + threadIdx.x;
                    if (tid < n) 
                    {
                        out[tid] = a * x[tid] + y[tid];
                    }
                }
                ";
                source += Environment.NewLine;

                var name = "Test";
                var headers = new string[0];
                var includeNames = new string[0];

                var compiler = new CudaRuntimeCompiler(source, name, headers, includeNames);

                //var compiler2 = new CudaRuntimeCompiler(source, name, headers, includeNames);
                // --ptxas-options=-v -keep
                compiler.Compile(new string[] { "-G" });

                //var ptxString = compiler.GetPTXAsString(); // for debugging

                var ptx = compiler.GetPTX();

                //compiler2.Compile(new string[] { });

                var kernel = ctx.LoadKernelPTX(ptx, "kernelName");

                //One kernel per cu file:
                //CudaKernel kernel = ctx.LoadKernel(@"path\to\kernel.ptx", "kernelname");
                kernel.GridDimensions = new dim3(1, 1, 1);
                kernel.BlockDimensions = new dim3(16, 16);

                //kernel.Run()

                var a = new CudaDeviceVariable<double>(100);
                //ManagedCuda.NPP.NPPsExtensions.NPPsExtensionMethods.Sqr()

                //Multiple kernels per cu file:
                CUmodule cumodule = ctx.LoadModule(@"path\to\kernel.ptx");
                CudaKernel kernel1 = new CudaKernel("kernel1", cumodule, ctx)
                {
                    GridDimensions = new dim3(1, 1, 1),
                    BlockDimensions = new dim3(16, 16),
                };
                CudaKernel kernel2 = new CudaKernel("kernel2", cumodule, ctx)
                {
                    GridDimensions = new dim3(1, 1, 1),
                    BlockDimensions = new dim3(16, 16),
                };

            }
        }
        }
}
