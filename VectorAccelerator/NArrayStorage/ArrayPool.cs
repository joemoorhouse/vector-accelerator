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
        SortedDictionary<int, ArrayPoolStack<double>> _storage = new SortedDictionary<int, ArrayPoolStack<double>>();
        const int maxSize = 100000;

        public double[] GetFromPool(int length)
        {
            var stack = GetStack(length);
            return stack.Pop();
        }

        public void AddToPool(double[] array)
        {
            var stack = GetStack(array.Length);
            stack.Push(array);
        }

        public ArrayPoolStack<double> GetStack(int length)
        {
            ArrayPoolStack<double> stack;
            if (!_storage.TryGetValue(length, out stack))
            {
                stack = new ArrayPoolStack<double>(length);
                _storage[length] = stack;
            }
            return stack;
        }
    }

    public class ArrayPoolStack<T>
    {
        private readonly Stack<T[]> _stack;
        private readonly int _arrayLength;

        public ArrayPoolStack(int arrayLength)
        {
            _stack = new Stack<T[]>();
            _arrayLength = arrayLength;
        }

        public int Count
        {
            get { return _stack.Count; }
        }

        public T[] Pop()
        {
            lock (_stack)
            {
                if (_stack.Count == 0)
                {
                    return new T[_arrayLength];
                }
                else return _stack.Pop();
            }
        }

        public void Push(T[] array)
        {
            lock (_stack)
            {
                if (array.Length != _arrayLength)
                    throw new ArgumentException("length mismatch", "array");
                _stack.Push(array);
            }
        }
    }
}
