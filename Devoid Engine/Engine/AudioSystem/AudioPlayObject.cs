using System.Numerics;

namespace DevoidEngine.Engine.AudioSystem
{
    public class AudioPlayObject
    {
        internal AudioPlayHandle Handle;
        public AudioClipHandle Clip;
        public Vector3 Position;
        public float Volume;
        public bool Loop;
        public bool Is3D;

        public float minDistance;
        public float maxDistance;
        public AudioAttenuation attenuationFunc;

        public Action? OnFinished;
    }
}