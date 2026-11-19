using System.Numerics;

namespace DevoidEngine.Audio
{
    public sealed class AudioManager : IDisposable
    {
        internal IAudioBackend Backend;
        private bool _disposed;

        internal AudioManager(IAudioBackend backend)
        {
            Backend = backend;
            Backend.Initialize();
        }

        public AudioClipHandle Load(string path)
        {
            return Backend.Load(path);
        }

        public AudioClipHandle Load(ReadOnlySpan<byte> data)
        {
            return Backend.Load(data);
        }

        public AudioPlayObject? Play(in AudioPlayDescription desc)
        {
            return Backend.Play(desc);
        }

        public void PlayOneShot(in AudioPlayDescription desc)
        {
            Backend.PlayOneShot(desc);
        }

        public void Stop(AudioPlayObject playObject)
        {
            Backend.Stop(playObject);
        }

        public void Pause(AudioPlayObject playObject, bool value = true)
        {
            Backend.Pause(playObject, value);
        }

        public void SetListener(Vector3 position, Vector3 forward, Vector3 up)
        {
            Backend.SetListener(position, forward, up);
        }

        public void QueueDelete(AudioClipHandle handle)
        {

        }

        public void Update()
        {
            Backend.Update();
        }


        public void Dispose()
        {
            if (_disposed)
                return;

            Backend?.Dispose();

            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
