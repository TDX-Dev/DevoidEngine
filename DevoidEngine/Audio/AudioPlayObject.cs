using System.Numerics;

namespace DevoidEngine.Audio
{
    public sealed class AudioPlayObject
    {
        internal AudioPlayHandle Handle;

        public AudioClipHandle Clip { get; init; }

        private Vector3 position;
        public Vector3 Position
        {
            get => position;
            set
            {
                if (position == value)
                    return;

                position = value;
                IsDirty = true;
            }
        }

        private float volume = 1.0f;
        public float Volume
        {
            get => volume;
            set
            {
                if (volume == value)
                    return;

                volume = value;
                IsDirty = true;
            }
        }

        private bool loop;
        public bool Loop
        {
            get => loop;
            set
            {
                if (loop == value)
                    return;

                loop = value;
                IsDirty = true;
            }
        }

        public bool Is3D { get; init; }

        private float minDistance = 1.0f;
        public float MinDistance
        {
            get => minDistance;
            set
            {
                if (minDistance == value)
                    return;

                minDistance = value;
                IsDirty = true;
            }
        }

        private float maxDistance = 50.0f;
        public float MaxDistance
        {
            get => maxDistance;
            set
            {
                if (maxDistance == value)
                    return;

                maxDistance = value;
                IsDirty = true;
            }
        }

        private AudioAttenuation attenuation = AudioAttenuation.InverseDistance;
        public AudioAttenuation Attenuation
        {
            get => attenuation;
            set
            {
                if (attenuation == value)
                    return;

                attenuation = value;
                IsDirty = true;
            }
        }

        public Action? OnFinished;

        internal bool IsDirty { get; set; } = true;
    }
}