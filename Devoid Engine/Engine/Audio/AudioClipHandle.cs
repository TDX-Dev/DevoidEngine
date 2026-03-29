namespace DevoidEngine.Engine.Audio
{
    public readonly struct AudioClipHandle
    {
        public readonly uint Id;

        internal AudioClipHandle(uint id)
        {
            Id = id;
        }
    }
}