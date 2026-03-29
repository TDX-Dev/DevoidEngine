using System.Numerics;

namespace DevoidEngine.Engine.Audio
{
    public sealed class AudioSystem : IDisposable
    {
        internal IAudioBackend _backend;
        private bool _disposed;

        internal AudioSystem(IAudioBackend backend)
        {
            _backend = backend;
            _backend.Initialize();
        }

        public AudioClipHandle Load(string path)
        {
            return _backend.Load(path);
        }

        public AudioPlayObject Play3D(AudioClipHandle clip, Vector3 position, bool loop = false)
        {
            return _backend.Play3D(clip, position, loop);
        }

        public void Stop(AudioPlayObject playObject)
        {
            _backend.Stop(playObject);
        }

        public void Pause(AudioPlayObject playObject, bool value = true)
        {
            _backend.Pause(playObject, value);
        }

        public void SetListener(Vector3 position, Vector3 forward, Vector3 up)
        {
            _backend.SetListener(position, forward, up);
        }

        public void Update()
        {
            _backend.Update();
        }


        public void Dispose()
        {
            if (_disposed)
                return;

            _backend?.Dispose();
            _backend = null;

            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}