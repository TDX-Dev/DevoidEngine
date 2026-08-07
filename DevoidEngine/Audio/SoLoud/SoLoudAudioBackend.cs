using SoLoud;
using System.Numerics;

namespace DevoidEngine.Audio.SoLoud
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
            _audioObjectMapping = [];
            _audioPlayObjects = [];
        }

        public void Initialize()
        {
            var result = Soloud.init();
            if (result != 0)
                throw new Exception("SoLoud init failed");

            // IMPORTANT: initialize listener immediately
            Soloud.set3dListenerParameters(
                0, 0, 0,
                0, 0, -1,
                0, 1, 0
            );

            Soloud.update3dAudio();
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

                if (obj.IsDirty)
                {
                    Soloud.setLooping(obj.Handle.Id, obj.Loop ? 1 : 0);
                    Soloud.setVolume(obj.Handle.Id, obj.Volume);

                    if (obj.Is3D)
                    {
                        var p = ToSoLoud(obj.Position);

                        Soloud.set3dSourcePosition(
                            obj.Handle.Id,
                            p.X,
                            p.Y,
                            p.Z);
                        Soloud.set3dSourceMinMaxDistance(
                            obj.Handle.Id,
                            obj.MinDistance,
                            obj.MaxDistance);

                        Soloud.set3dSourceAttenuation(
                            obj.Handle.Id,
                            (uint)obj.Attenuation,
                            1.0f);
                    }

                    obj.IsDirty = false;
                }
            }

            Soloud.update3dAudio();
        }

        public void SetListener(Vector3 position, Vector3 forward, Vector3 up)
        {
            position = ToSoLoud(position);
            forward = ToSoLoud(forward);
            up = ToSoLoud(up);

            Soloud.set3dListenerParameters(
                position.X, position.Y, position.Z,
                forward.X, forward.Y, forward.Z,
                up.X, up.Y, up.Z
            );
        }

        private static Vector3 ToSoLoud(Vector3 v)
        {
            return new Vector3(v.X, v.Y, -v.Z);
        }

        public AudioClipHandle Load(string path, bool stream)
        {
            if (stream)
            {
                throw new NotImplementedException("[Audio/SoLoudBackend]: Audio streaming not supported yet.");
            }

            AudioClipHandle audioHandle = new(++_nextAudioId);
            Wav audio = new();
            int result = audio.load(path);
            if (result != 0)
                throw new Exception("Audio Load Failed");


            _audioObjectMapping[audioHandle.Id] = audio;
            return audioHandle;
        }

        public unsafe AudioClipHandle Load(ReadOnlySpan<byte> data)
        {
            AudioClipHandle audioHandle = new(++_nextAudioId);

            Wav audio = new();

            fixed (byte* ptr = data)
            {
                int result = audio.loadMem((IntPtr)ptr, (uint)data.Length, 1, 0);

                if (result != 0)
                    throw new Exception("Audio Load Failed");
            }

            _audioObjectMapping[audioHandle.Id] = audio;
            return audioHandle;
        }

        public AudioPlayObject? Play(in AudioPlayDescription desc)
        {
            uint voice = PlayVoice(desc);

            if (voice == 0)
                return null;

            var playObject = new AudioPlayObject
            {
                Handle = new AudioPlayHandle(voice),
                Clip = desc.Clip,

                Position = desc.Position,
                Volume = desc.Volume,
                Loop = desc.Loop,

                MinDistance = desc.MinDistance,
                MaxDistance = desc.MaxDistance,
                Attenuation = desc.Attenuation,

                Is3D = desc.Is3D
            };

            _audioPlayObjects.Add(playObject);

            return playObject;
        }

        public void PlayOneShot(in AudioPlayDescription desc)
        {
            PlayVoice(desc);
        }

        private uint PlayVoice(in AudioPlayDescription desc)
        {
            if (!_audioObjectMapping.TryGetValue(desc.Clip.Id, out var wav))
                return 0;

            uint voice;

            if (desc.Is3D)
            {
                var p = ToSoLoud(desc.Position);

                voice = Soloud.play3d(
                    wav,
                    p.X,
                    p.Y,
                    p.Z);

                Soloud.set3dSourceMinMaxDistance(
                    voice,
                    desc.MinDistance,
                    desc.MaxDistance);

                Soloud.set3dSourceAttenuation(
                    voice,
                    (uint)desc.Attenuation,
                    1.0f);
            }
            else
            {
                voice = Soloud.play(wav);
            }

            Soloud.setVolume(voice, desc.Volume);
            Soloud.setLooping(voice, desc.Loop ? 1 : 0);

            return voice;
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
            Soloud?.stopAll();

            _audioObjectMapping?.Clear();

            _audioPlayObjects?.Clear();

            Soloud?.deinit();
        }
    }
}
