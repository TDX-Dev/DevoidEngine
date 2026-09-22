using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Rendering.PostProcessing
{
    public sealed class PostProcessSettings
    {
        public static PostProcessSettings Default { get; set; } = new()
        {
            BloomEnabled = false,
            AnamorphicBloomEnabled = false,
        };

        public bool BloomEnabled { get; set; } = true;
        public bool AnamorphicBloomEnabled { get; set; } = true;

        public float Exposure { get; set; } = 0.6f;
        public float BloomIntensity { get; set; } = 1.0f;
        public float AnamorphicBloomIntensity { get; set; } = 1.0f;

        public float BloomThreshold = 1f;
        public float BloomKnee = 0.5f;
    }
}
