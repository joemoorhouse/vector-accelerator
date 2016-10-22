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
        //note no storage start offset: if this is a view into another vector / matrix,
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

        public DeviceStorage(CudaDeviceVariable<T> storage, int stride)
        {
            _storage = storage;
            _stride = stride;
        }

        private static CudaDeviceVariable<S> Offset<S>(CudaDeviceVariable<S> original,
            int offset) where S : struct
        {
            var typeSize = (uint)Marshal.SizeOf(typeof(S));
            return new CudaDeviceVariable<S>(original.DevicePointer + typeSize * offset);
        }
    }
}
