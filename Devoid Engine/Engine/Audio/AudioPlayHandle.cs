namespace DevoidEngine.Engine.Audio
{
    public readonly struct AudioPlayHandle
    {
        public readonly uint Id;

        internal AudioPlayHandle(uint id)
        {
            Id = id;
        }
    }
}