using DevoidEngine.Engine.Audio;

namespace DevoidEngine.Engine.Components
{
    public class AudioSourceComponent3D : Component
    {
        public override string Type => nameof(AudioSourceComponent3D);

        // --- Config ---
        public string AudioPath;
        public bool PlayOnStart = true;
        public bool Looping = false;

        public float Volume = 1.0f;
        public float MinDistance = 1.0f;
        public float MaxDistance = 50.0f;

        // --- Runtime ---
        private AudioClipHandle clip;
        private AudioPlayObject player;

        public bool IsPlaying => player != null;

        public override void OnStart()
        {
            if (string.IsNullOrEmpty(AudioPath))
                return;

            clip = gameObject.Scene.Audio.Load(AudioPath);

            if (PlayOnStart)
                Play();
        }

        public override void OnUpdate(float dt)
        {
            if (player == null) return;

            // update position every frame
            player.Position = gameObject.Transform.Position;
        }

        public override void OnDestroy()
        {
            Stop();
        }

        // --- Controls ---

        public void Play()
        {
            if (clip.Id == 0)
                clip = gameObject.Scene.Audio.Load(AudioPath);

            Stop(); // restart cleanly

            player = gameObject.Scene.Audio.Play3D(
                clip,
                gameObject.Transform.Position,
                Looping
            );

            ApplySettings();
        }

        public void Stop()
        {
            if (player == null) return;

            gameObject.Scene.Audio.Stop(player);
            player = null;
        }

        public void Pause()
        {
            gameObject.Scene.Audio.Pause(player);
        }

        public void Resume()
        {
            gameObject.Scene.Audio.Pause(player, false);
        }

        // --- Settings ---

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
            player.minDistance = MinDistance;
            player.maxDistance = MaxDistance;
        }
    }
}