using DevoidEngine.Engine.AudioSystem;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.GizmoSystem;
using DevoidEngine.Engine.Rendering;
using System.Numerics;

namespace DevoidEngine.Engine.Components
{
    public class AudioSourceComponent3D : Component
    {
        public override string Type => nameof(AudioSourceComponent3D);

        public bool PlayOnStart = true;
        public bool Looping = false;

        public float Volume
        {
            get => volume;
            set
            {
                volume = value;
                ApplySettings();
            }
        }

        public float MinDistance
        {
            get => minDistance;
            set
            {
                minDistance = value;
                ApplySettings();
            }
        }

        public float MaxDistance
        {
            get => maxDistance;
            set
            {
                maxDistance = value;
                ApplySettings();
            }
        }

        public AudioAttenuation Attenuation
        {
            get => attenuation;
            set
            {
                attenuation = value;
                ApplySettings();
            }
        }

        internal float volume = 1.0f;
        internal float minDistance = 1.0f;
        internal float maxDistance = 50.0f;
        internal AudioAttenuation attenuation = AudioAttenuation.LinearDistance;
        public AudioClip? Audio;

        private AudioPlayObject? player;

        public bool IsPlaying => player != null;

        public override void OnStart()
        {
            if (Audio == null)
                return;

            if (PlayOnStart)
                Play();
        }

        public override void OnUpdate(float dt)
        {
            if (player == null) return;

            player.Position = gameObject.Transform.Position;
        }

        public override void OnRender()
        {
            if (Camera.Main == null)
                return;
            Matrix4x4 model = Matrix4x4.CreateScale(maxDistance) * Matrix4x4.CreateTranslation(gameObject.Transform.Position);
            Gizmos.DrawSphere(model, GizmoCategory.Audio);

            //Matrix4x4 model = GizmoHelper.BillboardCircle(gameObject.Transform.Position, maxDistance, Camera.Main.Position);
            //Gizmos.DrawCircle(model, GizmoCategory.Audio);
        }

        public override void OnDestroy()
        {
            Stop();
        }


        public void Play()
        {
            if (Audio == null)
            {
                Console.WriteLine("Cannot play without audio clip");
                return;
            }

            Stop();

            var desc = new AudioPlayDescription
            {
                Clip = Audio._handle,
                Position = gameObject.Transform.Position,

                Volume = Volume,
                Loop = Looping,

                MinDistance = MinDistance,
                MaxDistance = MaxDistance,

                Attenuation = attenuation,
                Is3D = true
            };

            player = gameObject.Scene.Audio.Play(desc);
        }

        public void Stop()
        {
            if (player == null) return;

            gameObject.Scene.Audio.Stop(player);
            player = null;
        }

        public void Pause()
        {
            if (player == null) return;
            gameObject.Scene.Audio.Pause(player);
        }

        public void Resume()
        {
            if (player == null) return;
            gameObject.Scene.Audio.Pause(player, false);
        }


        public void SetVolume(float volume)
        {
            Volume = volume;
            if (player != null)
                player.Volume = volume;
        }

        public void SetLooping(bool looping)
        {
            Looping = looping;
            if (player != null)
                player.Loop = looping;
        }

        public void SetDistance(float min, float max)
        {
            MinDistance = min;
            MaxDistance = max;

            if (player != null)
            {
                player.minDistance = min;
                player.maxDistance = max;
            }
        }

        private void ApplySettings()
        {
            if (player == null) return;

            player.Volume = Volume;
            player.Loop = Looping;
            player.minDistance = minDistance;
            player.maxDistance = maxDistance;
            player.attenuationFunc = attenuation;
        }
    }
}