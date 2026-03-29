using System.Numerics;

namespace DevoidEngine.Engine.Audio
{
    internal interface IAudioBackend : IDisposable
    {
        void Initialize();
        void Update();
        void SetListener(Vector3 position, Vector3 forward, Vector3 up);
        AudioClipHandle Load(string path, bool stream = false);
        AudioPlayObject Play3D(AudioClipHandle clip, Vector3 pos, bool loop = false);
        void Stop(AudioPlayObject playObject);
        void Pause(AudioPlayObject playObject, bool value = true);
    }
}