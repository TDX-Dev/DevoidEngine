using System.Numerics;

namespace DevoidEngine.Engine.AudioSystem
{
    internal interface IAudioBackend : IDisposable
    {
        void Initialize();
        void Update();
        void SetListener(Vector3 position, Vector3 forward, Vector3 up);
        AudioClipHandle Load(string path, bool stream = false);
        AudioClipHandle Load(ReadOnlySpan<byte> data);
        AudioPlayObject? Play(in AudioPlayDescription desc);
        void Stop(AudioPlayObject playObject);
        void Pause(AudioPlayObject playObject, bool value = true);
    }
}