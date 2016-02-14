using System;
using System.Collections;
using System.Collections.Generic;

namespace Chess.Core
{
    public sealed class CircularArray<T> : IEnumerator<T>
    {
        private readonly T[] array;
        public int Index { get; set; }

        public T Current
        {
            get { return array[Index]; }
        }

        public CircularArray(T[] array)
        {
            Index = -1;
            this.array = array;
        }

        public int IndexOf(T value)
        {
            return Array.IndexOf(array, value);
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            if (++Index >= array.Length)
                Index = 0;
            return true;
        }

        public void Reset()
        {
            Index = -1;
        }

        public void Dispose()
        {
        }
    }
}