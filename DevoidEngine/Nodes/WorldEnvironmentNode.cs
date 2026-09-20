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
        public bool AnamorphicBloom
        {
            get => PostProcessSettings.AnamorphicBloomEnabled;
            set
            {
                PostProcessSettings.AnamorphicBloomEnabled = value;
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
