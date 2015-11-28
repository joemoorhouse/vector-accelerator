using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManagedCuda;
using System.Runtime.InteropServices;

namespace VectorAccelerator.NArrayStorage
{
    public class DeviceStorage<T> where T : struct //: NArrayStorage<T> 
    {
        CudaDeviceVariable<T> _storage; // column-major storage
        // note no storgae start offset: if this is a view into another vector / matrix,
        // this is done in the pointer
        int _stride; // number of elements between adjacent columns of same row

        public CudaDeviceVariable<T> Array
        {
            get { return _storage; }
        }

        public int Stride
        {
            get { return _stride; }
        }

        private static CudaDeviceVariable<T> Offset<T>(CudaDeviceVariable<T> original,
            int offset) where T : struct
        {
            var typeSize = (uint)Marshal.SizeOf(typeof(T));
            return new CudaDeviceVariable<T>(original.DevicePointer + typeSize * offset); 
        }
    }
}
