using System;
using System.IO;
using System.Reflection;

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
            string newPath = Path.Combine(AssemblyDirectory, Environment.Is64BitProcess ? "x64" : "x86");
            System.Environment.SetEnvironmentVariable("PATH",
                string.Join(";", newPath, path));
        }

        public static string AssemblyDirectory
        {
            get
            {
                //Environment.CurrentDirectory
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        private static object locker = new object();
        private static bool _alreadyCalled = false;
    }
}
