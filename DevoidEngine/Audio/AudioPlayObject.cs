using System.Numerics;

namespace DevoidEngine.Audio
{
    public sealed class AudioPlayObject
    {
        internal AudioPlayHandle Handle;

        public AudioClipHandle Clip { get; init; }

        private Vector3 _position;
        public Vector3 Position
        {
            get => _position;
            set
            {
                if (_position == value)
                    return;

                _position = value;
                IsDirty = true;
            }
        }

        private float _volume = 1.0f;
        public float Volume
        {
            get => _volume;
            set
            {
                if (_volume == value)
                    return;

                _volume = value;
                IsDirty = true;
            }
        }

        private bool _loop;
        public bool Loop
        {
            get => _loop;
            set
            {
                if (_loop == value)
                    return;

                _loop = value;
                IsDirty = true;
            }
        }

        public bool Is3D { get; init; }

        private float _minDistance = 1.0f;
        public float MinDistance
        {
            get => _minDistance;
            set
            {
                if (_minDistance == value)
                    return;

                _minDistance = value;
                IsDirty = true;
            }
        }

        private float _maxDistance = 50.0f;
        public float MaxDistance
        {
            get => _maxDistance;
            set
            {
                if (_maxDistance == value)
                    return;

                _maxDistance = value;
                IsDirty = true;
            }
        }

        private AudioAttenuation _attenuation = AudioAttenuation.InverseDistance;
        public AudioAttenuation Attenuation
        {
            get => _attenuation;
            set
            {
                if (_attenuation == value)
                    return;

                _attenuation = value;
                IsDirty = true;
            }
        }

        public Action? OnFinished;

        internal bool IsDirty { get; set; } = true;
    }
}