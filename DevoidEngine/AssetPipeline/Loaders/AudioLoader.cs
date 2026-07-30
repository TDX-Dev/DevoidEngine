using DevoidEngine.Audio;
using DevoidEngine.Core;

namespace DevoidEngine.AssetPipeline.Loaders
{
    internal class AudioLoader : IAssetLoader<AudioClip>
    {
        public string RuntimeExtension => "audio";

        public AudioClip Load(ReadOnlySpan<byte> data)
        {
            AudioClip audio = new()
            {
                _handle = Engine.AudioSystem.Load(data)
            };
            return audio;
        }
    }
}
