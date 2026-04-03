namespace DevoidEngine.Engine.AudioSystem
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