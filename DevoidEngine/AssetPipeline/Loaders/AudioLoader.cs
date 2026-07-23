using DevoidEngine.Audio;
using DevoidEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.AssetPipeline.Loaders
{
    internal class AudioLoader : IAssetLoader<AudioClip>
    {
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
