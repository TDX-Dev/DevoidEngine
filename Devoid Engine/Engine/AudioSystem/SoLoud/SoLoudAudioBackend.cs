using SoLoud;
using System.Numerics;

namespace DevoidEngine.Engine.AudioSystem.SoLoud
{
    internal class SoLoudAudioBackend : IAudioBackend
    {
        internal Dictionary<uint, Wav> _audioObjectMapping;
        internal uint _nextAudioId = 0;

        internal List<AudioPlayObject> _audioPlayObjects;

        internal Soloud Soloud;

        public SoLoudAudioBackend()
        {
            Soloud = new Soloud();
            _audioObjectMapping = new Dictionary<uint, Wav>();
            _audioPlayObjects = new List<AudioPlayObject>();
        }

        public void Initialize()
        {
            var result = Soloud.init();
            if (result != 0)
                throw new Exception("SoLoud init failed");
        }

        public void Update()
        {
            for (int i = _audioPlayObjects.Count - 1; i >= 0; i--)
            {
                var obj = _audioPlayObjects[i];

                // Remove if finished
                if (Soloud.isValidVoiceHandle(obj.Handle.Id) == 0)
                {
                    _audioPlayObjects[i].OnFinished?.Invoke();
                    _audioPlayObjects.RemoveAt(i);
                    continue;
                }

                // Apply volume
                Soloud.setLooping(obj.Handle.Id, obj.Loop ? 1 : 0);
                Soloud.setVolume(obj.Handle.Id, obj.Volume);
                Soloud.set3dSourceMinMaxDistance(obj.Handle.Id, obj.minDistance, obj.maxDistance);
                Soloud.set3dSourceAttenuation(obj.Handle.Id, (uint)obj.attenuationFunc, 1.0f);

                // Apply position
                if (obj.Is3D)
                {
                    Soloud.set3dSourcePosition(
                        obj.Handle.Id,
                        obj.Position.X,
                        obj.Position.Y,
                        obj.Position.Z
                    );
                }
            }

            Soloud.update3dAudio();
        }

        public void SetListener(Vector3 position, Vector3 forward, Vector3 up)
        {
            Soloud.set3dListenerParameters(
                position.X, position.Y, position.Z,
                forward.X, forward.Y, forward.Z,
                up.X, up.Y, up.Z
            );
        }

        public AudioClipHandle Load(string path, bool stream)
        {
            if (stream)
            {
                throw new NotImplementedException("[Audio/SoLoudBackend]: Audio streaming not supported yet.");
            }

            AudioClipHandle audioHandle = new AudioClipHandle(++_nextAudioId);
            Wav audio = new Wav();
            int result = audio.load(path);
            if (result != 0)
                throw new Exception("Audio Load Failed");


            _audioObjectMapping[audioHandle.Id] = audio;
            return audioHandle;
        }

        public unsafe AudioClipHandle Load(ReadOnlySpan<byte> data)
        {
            AudioClipHandle audioHandle = new AudioClipHandle(++_nextAudioId);

            Wav audio = new Wav();

            fixed (byte* ptr = data)
            {
                int result = audio.loadMem((IntPtr)ptr, (uint)data.Length, 1, 0);

                if (result != 0)
                    throw new Exception("Audio Load Failed");
            }

            _audioObjectMapping[audioHandle.Id] = audio;
            return audioHandle;
        }

        public AudioPlayObject? Play3D(AudioClipHandle clip, Vector3 pos, bool loop = false)
        {
            if (!_audioObjectMapping.TryGetValue(clip.Id, out var wav))
                return null;

            uint voice = Soloud.play3d(wav, pos.X, pos.Y, pos.Z);

            Soloud.setLooping(voice, loop ? 1 : 0);
            var playObject = new AudioPlayObject
            {
                Handle = new AudioPlayHandle(voice),
                Clip = clip,
                Position = pos,
                Volume = 1.0f,
                Is3D = true,
                Loop = loop
            };

            _audioPlayObjects.Add(playObject);

            return playObject;
        }

        public void Stop(AudioPlayObject playObject)
        {
            if (playObject == null) return;

            Soloud.stop(playObject.Handle.Id);
            _audioPlayObjects.Remove(playObject);
        }

        public void Pause(AudioPlayObject playObject, bool value = true)
        {
            if (playObject == null) return;
            Soloud.setPause(playObject.Handle.Id, value ? 1 : 0);
        }

        public void Dispose()
        {
            // 1. Stop everything first
            Soloud?.stopAll();

            // 2. Dispose all Wavs explicitly
            if (_audioObjectMapping != null)
            {
                _audioObjectMapping.Clear();
            }

            // 3. Clear play objects (they hold handles)
            _audioPlayObjects?.Clear();

            // 4. Now safely shutdown SoLoud
            Soloud?.deinit();
        }
    }
}