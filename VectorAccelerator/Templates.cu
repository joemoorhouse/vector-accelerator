extern "C" __global__ 
    void ZipExample(float** a, float** b, float* c, float** result, int n)
    {
        size_t tid = blockIdx.x * blockDim.x + threadIdx.x; 
        if (tid < n) 
        {
            float local0 = 0;
			for (int i = 0; i < n; ++i)
			{
				local0 = a[i][tid] * b[i][tid] * c[i];
			}
			result[tid] = local0;
        }
    }

	