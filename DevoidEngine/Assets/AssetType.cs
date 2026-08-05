using System;
using System.Threading;

namespace DevoidEngine.Assets
{
    public abstract class AssetType : IDisposable
    {
        public Guid Guid;

        // 1. Create a private backing field
        private int _refCount;

        // 2. Expose it via a read-only property for the rest of your engine
        public int RefCount => _refCount;

        public void Retain()
        {
            // 3. Pass the private backing field by ref
            Interlocked.Increment(ref _refCount);
        }

        public bool Release()
        {
            // 4. Pass the private backing field by ref
            int newCount = Interlocked.Decrement(ref _refCount);

            if (newCount <= 0)
            {
                // The asset is no longer used by anything. 
                // Return true so the caller knows it should be removed from cache.
                return true;
            }
            return false;
        }

        public abstract void Dispose();
    }
}