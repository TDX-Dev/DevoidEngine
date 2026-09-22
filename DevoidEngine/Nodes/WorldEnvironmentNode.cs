using DevoidEngine.Rendering.PostProcessing;

namespace DevoidEngine.Nodes
{
    public class WorldEnvironmentNode : Node
    {
        public PostProcessSettings PostProcessSettings { get; } = new();

        public bool Bloom
        {
            get => PostProcessSettings.BloomEnabled;
            set
            {
                PostProcessSettings.BloomEnabled = value;
            }
        }
        public float BloomIntensity
        {
            get => PostProcessSettings.BloomIntensity;
            set
            {
                PostProcessSettings.BloomIntensity = value;
            }
        }
        public float BloomThreshold
        {
            get => PostProcessSettings.BloomThreshold;
            set
            {
                PostProcessSettings.BloomThreshold = value;
            }
        }
        public float BloomKnee
        {
            get => PostProcessSettings.BloomKnee;
            set
            {
                PostProcessSettings.BloomKnee = value;
            }
        }
        public bool AnamorphicBloom
        {
            get => PostProcessSettings.AnamorphicBloomEnabled;
            set
            {
                PostProcessSettings.AnamorphicBloomEnabled = value;
            }
        }
        public float AnamorphicBloomIntensity
        {
            get => PostProcessSettings.AnamorphicBloomIntensity;
            set
            {
                PostProcessSettings.AnamorphicBloomIntensity = value;
            }
        }
        public float Exposure
        {
            get => PostProcessSettings.Exposure;
            set
            {
                PostProcessSettings.Exposure = value;
            }
        }
        protected override void OnAttach()
        {
            Scene?.RegisterWorldEnvironment(this);
        }

        protected override void OnDestroy()
        {
            Scene?.ClearWorldEnvironment(this);
        }
    }
}
