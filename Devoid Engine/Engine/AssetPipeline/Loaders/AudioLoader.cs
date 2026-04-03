using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.AssetPipeline.Loaders
{
    internal class AudioLoader : IAssetLoader<Audio>
    {
        public Audio Load(ReadOnlySpan<byte> data)
        {
            var audio = new Audio();
            audio.audioClip = EngineSingleton.Instance.AudioSystem.Load(data);
            return audio;
        }
    }
}
