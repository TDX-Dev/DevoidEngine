namespace DevoidEngine.Util
{
    public struct Slot<T>
    {
        public T Value;
        public uint Generation;
        public bool Occupied;
    }
}
