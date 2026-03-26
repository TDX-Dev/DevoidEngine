namespace DevoidGPU
{
    public static class TextureManager
    {
        private static Dictionary<IntPtr, ITexture> map = new();
        private static int nextId = 1;

        public static IntPtr Register(ITexture tex)
        {
            var id = new IntPtr(nextId++);
            map[id] = tex;
            return id;
        }

        public static ITexture Resolve(IntPtr id)
        {
            return map[id];
        }

        public static void Unregister(IntPtr id)
        {
            map.Remove(id);
        }
    }

}