using System.Numerics;

namespace DevoidEngine.Engine.AudioSystem
{
    public sealed class AudioManager : IDisposable
    {
        internal IAudioBackend _backend;
        private bool _disposed;

        internal AudioManager(IAudioBackend backend)
        {
            _backend = backend;
            _backend.Initialize();
        }

        public AudioClipHandle Load(string path)
        {
            return _backend.Load(path);
        }

        public AudioClipHandle Load(ReadOnlySpan<byte> data)
        {
            return _backend.Load(data);
        }

        public AudioPlayObject? Play(in AudioPlayDescription desc)
        {
            return _backend.Play(desc);
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

            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}