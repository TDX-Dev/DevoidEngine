using System.Runtime.InteropServices;

namespace DevoidEngine.Util
{
    public sealed class SparseListSet<T>
    {
        private readonly List<T> _dense = [];
        private readonly List<int> _sparse = [];
        private readonly List<int> _denseToSparse = [];

        public int Count => _dense.Count;

        public bool Contains(int id)
        {
            return (uint)id < (uint)_sparse.Count &&
                   (uint)_sparse[id] < (uint)_dense.Count &&
                   _denseToSparse[_sparse[id]] == id;
        }

        public void Add(int id, T value)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(id);

            if (Contains(id))
                throw new InvalidOperationException("ID already exists.");

            EnsureSparseCapacity(id + 1);

            int index = _dense.Count;

            _sparse[id] = index;
            _denseToSparse.Add(id);
            _dense.Add(value);
        }

        public bool Remove(int id)
        {
            if (!Contains(id))
                return false;

            int index = _sparse[id];
            int lastIndex = _dense.Count - 1;

            if (index != lastIndex)
            {
                T movedValue = _dense[lastIndex];
                int movedId = _denseToSparse[lastIndex];

                _dense[index] = movedValue;
                _denseToSparse[index] = movedId;
                _sparse[movedId] = index;
            }

            _dense.RemoveAt(lastIndex);
            _denseToSparse.RemoveAt(lastIndex);
            _sparse[id] = -1;

            return true;
        }

        public bool TryGetValue(int id, out T value)
        {
            if (Contains(id))
            {
                value = _dense[_sparse[id]];
                return true;
            }

            value = default!;
            return false;
        }

        public Span<T> AsSpan() => CollectionsMarshal.AsSpan(_dense);
        public List<T> GetDenseList() => _dense;

        private void EnsureSparseCapacity(int capacity)
        {
            if (capacity <= _sparse.Count)
                return;

            int oldCount = _sparse.Count;
            int newCapacity = Math.Max(capacity, Math.Max(4, oldCount * 2));

            _sparse.EnsureCapacity(newCapacity);

            for (int i = oldCount; i < newCapacity; i++)
                _sparse.Add(-1);
        }
    }

}
