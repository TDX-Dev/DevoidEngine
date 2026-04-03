using DevoidEngine.Engine.AssetPipeline;
using DevoidEngine.Engine.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Core
{
    public class EngineSingleton
    {
        public uint FrameCount { get; set; } = 0;
        public float InterpolationAlpha { get; set; } = 0;
        public bool UseInterpolation = false;

        public AudioSystem AudioSystem { get; set; }
        public VirtualFileSystem VirtualFileSystem { get; set; }




        public static EngineSingleton Instance { get; private set; }

        public EngineSingleton()
        {
            if (Instance != null)
                throw new Exception("Cannot create multiple engine singletons");

            Instance = this;
        }

    }
}
