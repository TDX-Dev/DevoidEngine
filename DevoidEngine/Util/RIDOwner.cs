using System.Runtime.InteropServices;

namespace DevoidEngine.Util
{
    public sealed class RIDOwner<T>
    {
        private readonly List<Slot<T>> _entries = [];
        private readonly Stack<int> _freeList = [];

        public void FreeAll()
        {
            _freeList.Clear();
            _entries.Clear();
        }

        public void FreeWhere(Func<T, bool> predicate)
        {
            Span<Slot<T>> entries = CollectionsMarshal.AsSpan(_entries);

            for (int i = 0; i < entries.Length; i++)
            {
                ref Slot<T> entry = ref entries[i];

                if (!entry.Occupied || !predicate(entry.Value)) continue;

                Free(new RID(i, entry.Generation));
            }
        }

        public int GetCount()
        {
            return _entries.Count;
        }

        public RID MakeRID(T value)
        {
            int index;

            if (_freeList.Count > 0)
            {
                index = _freeList.Pop();

                var entry = _entries[index];
                entry.Value = value;
                entry.Occupied = true;
                _entries[index] = entry;
            }
            else
            {
                index = _entries.Count;

                _entries.Add(new Slot<T>
                {
                    Value = value,
                    Generation = 1,
                    Occupied = true
                });
            }

            return new RID(index, _entries[index].Generation);
        }

        public RID GetRID(int index)
        {
            Slot<T> slot = _entries[index];

            return new RID(index, slot.Generation);
        }

        public bool Owns(RID rid)
        {
            if (rid.Index < 0 || rid.Index >= _entries.Count)
                return false;

            var entry = _entries[rid.Index];

            return entry.Occupied &&
                   entry.Generation == rid.Generation;
        }

        public T Get(RID rid)
        {
            if (!Owns(rid))
            {
                throw new InvalidOperationException();
            }

            return _entries[rid.Index].Value;
        }

        public ref T GetRef(RID rid)
        {
            Span<Slot<T>> span = CollectionsMarshal.AsSpan(_entries);

            if ((uint)rid.Index >= (uint)span.Length)
                throw new InvalidOperationException();

            ref Slot<T> slot = ref span[rid.Index];

            if (!slot.Occupied ||
                slot.Generation != rid.Generation)
            {
                throw new InvalidOperationException();
            }

            return ref slot.Value;
        }

        public void Free(RID rid)
        {
            if (!Owns(rid))
                return;

            var entry = _entries[rid.Index];

            entry.Occupied = false;
            entry.Generation++;

            _entries[rid.Index] = entry;

            _freeList.Push(rid.Index);
        }

        public ReadOnlySpan<Slot<T>> AsSpan()
        {
            return CollectionsMarshal.AsSpan(_entries);
        }
    }
}
