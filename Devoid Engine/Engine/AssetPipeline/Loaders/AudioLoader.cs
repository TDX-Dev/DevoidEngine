using DevoidEngine.Engine.AudioSystem;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.AssetPipeline.Loaders
{
    internal class AudioLoader : IAssetLoader<AudioClip>
    {
        public AudioClip Load(ReadOnlySpan<byte> data)
        {
            var audio = new AudioClip();
            audio._handle = EngineSingleton.Instance.AudioSystem.Load(data);
            return audio;
        }
    }
}
