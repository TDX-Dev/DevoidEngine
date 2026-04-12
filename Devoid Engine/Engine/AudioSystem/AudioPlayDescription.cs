using System.Numerics;

namespace DevoidEngine.Engine.AudioSystem
{
    public struct AudioPlayDescription
    {
        public AudioClipHandle Clip;

        public Vector3 Position;

        public float Volume;
        public bool Loop;

        public float MinDistance;
        public float MaxDistance;

        public AudioAttenuation Attenuation;

        public bool Is3D;

        public static AudioPlayDescription Default3D(AudioClipHandle clip, Vector3 pos)
        {
            return new AudioPlayDescription
            {
                Clip = clip,
                Position = pos,

                Volume = 1.0f,
                Loop = false,

                MinDistance = 1.0f,
                MaxDistance = 50.0f,

                Attenuation = AudioAttenuation.InverseDistance,

                Is3D = true
            };
        }
    }
}