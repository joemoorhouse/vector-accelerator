using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace VectorAccelerator.LinearAlgebraProviders
{
    class NativeLibraryHelper
    {
        public static void AddLibraryPath()
        {
            lock (locker)
            {
                if (_alreadyCalled) return;
                _alreadyCalled = true;
            }
            string path = System.Environment.GetEnvironmentVariable("PATH");
            string newPath = Path.Combine(Environment.CurrentDirectory, Environment.Is64BitProcess ? "x64" : "x86");
            System.Environment.SetEnvironmentVariable("PATH",
                string.Join(";", newPath, path));
        }

        private static object locker = new object();
        private static bool _alreadyCalled = false;
    }
}
