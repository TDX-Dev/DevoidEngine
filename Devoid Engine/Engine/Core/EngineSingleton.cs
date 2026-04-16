using DevoidEngine.Engine.AssetPipeline;
using DevoidEngine.Engine.AudioSystem;
using DevoidEngine.Engine.Physics;
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

        public float TargetFrameRate { get; set; } = 60;

        public bool SimulatePhysics { get; set; } = true;

        public AudioManager AudioSystem { get; set; } = null!;
        public PhysicsSystem PhysicsSystem { get; set; } = null!;




        public static EngineSingleton Instance { get; private set; } = null!;

        public EngineSingleton()
        {
            if (Instance != null)
                throw new Exception("Cannot create multiple engine singletons");

            Instance = this;
        }

    }
}
