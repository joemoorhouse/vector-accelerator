using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator.NArrayStorage
{
    /// <summary>
    /// Class that keeps a pool of arrays commonly used for temporary variables
    /// </summary>
    public class ArrayPool
    {
        SortedDictionary<int, Stack<double[]>> _storage = new SortedDictionary<int, Stack<double[]>>();
        const int maxSize = 100000;

        public double[] GetFromPool(int length)
        {
            var stack = GetStack(length);
            double[] array;
            if (stack.Count > 0)
            {
                array = stack.Pop();
            }
            else
            {
                array = new double[length];
            }
            return array;
        }

        public void AddToPool(double[] array)
        {
            var stack = GetStack(array.Length);
            stack.Push(array);
        }

        private Stack<double[]> GetStack(int length)
        {
            Stack<double[]> stack;
            if (!_storage.TryGetValue(length, out stack))
            {
                stack = new Stack<double[]>();
                _storage[length] = stack;
            }
            return stack;
        }
    }
}
