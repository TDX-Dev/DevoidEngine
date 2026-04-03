namespace DevoidEngine.Engine.AudioSystem
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