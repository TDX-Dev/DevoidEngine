using DevoidEngine.Audio;
using DevoidEngine.Core;

namespace DevoidEngine.AssetPipeline.Loaders
{
    internal class AudioLoader : IAssetLoader<AudioClip>
    {
        public string RuntimeExtension => "audio";

        public AudioClip Load(byte[] data)
        {
            AudioClip audio = new(new AudioClipHandle());
            return audio;
        }
    }
}
