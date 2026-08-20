using System;
using System.Threading;

namespace DevoidEngine.Assets
{
    public abstract class AssetType : IDisposable
    {
        public Guid Guid;
        private int _refCount;
        public int RefCount => _refCount;

        public void Retain()
        {
            Interlocked.Increment(ref _refCount);
        }

        public bool Release()
        {
            int newCount = Interlocked.Decrement(ref _refCount);

            if (newCount <= 0)
            {
                return true;
            }
            return false;
        }

        public abstract void Dispose();
    }
}