using DevoidEngine.Audio;
using DevoidEngine.Core;

namespace DevoidEngine.AssetPipeline.Loaders
{
    internal class AudioLoader : IAssetLoader<AudioClip>
    {
        public string RuntimeExtension => "audio";

        public AudioClip Load(byte[] data)
        {
            AudioClip audio = new(Engine.AudioSystem.Load(new ReadOnlySpan<byte>(data)));
            return audio;
        }
    }
}
