namespace DevoidGPU
{
    public struct ShaderDescription
    {
        public string Name;
        public string Source;      // raw source
        public string FilePath;    // File path, preferred if we want include preprocessor working, otherwise add it in yourself and pass via source.
        public string EntryPoint;  // "main"
        public ShaderStage Stage;
        public Dictionary<string, string> Defines;
    }
}
