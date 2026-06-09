using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Util
{
    public sealed class RIDOwner<T>
    {
        private readonly List<Slot<T>> entries = [];
        private readonly Stack<int> freeList = [];

        public RID MakeRID(T value)
        {
            int index;

            if (freeList.Count > 0)
            {
                index = freeList.Pop();

                var entry = entries[index];
                entry.Value = value;
                entry.Occupied = true;
                entries[index] = entry;
            }
            else
            {
                index = entries.Count;

                entries.Add(new Slot<T>
                {
                    Value = value,
                    Generation = 1,
                    Occupied = true
                });
            }

            return new RID(index, entries[index].Generation);
        }

        public bool Owns(RID rid)
        {
            if (rid.Index < 0 || rid.Index >= entries.Count)
                return false;

            var entry = entries[rid.Index];

            return entry.Occupied &&
                   entry.Generation == rid.Generation;
        }

        public T Get(RID rid)
        {
            if (!Owns(rid))
                throw new InvalidOperationException();

            return entries[rid.Index].Value;
        }

        public void Free(RID rid)
        {
            if (!Owns(rid))
                return;

            var entry = entries[rid.Index];

            entry.Occupied = false;
            entry.Generation++;

            entries[rid.Index] = entry;

            freeList.Push(rid.Index);
        }

        public IEnumerable<(RID Rid, T Value)> Enumerate()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];

                if (!entry.Occupied)
                    continue;

                yield return (
                    new RID(i, entry.Generation),
                    entry.Value
                );
            }
        }
    }
}
