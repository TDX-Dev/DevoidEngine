using DevoidEngine.Assets;

namespace DevoidEngine.Audio
{
    public sealed class AudioClip : AssetType
    {
        internal AudioClipHandle _handle;

        internal AudioClip(AudioClipHandle handle)
        {
            _handle = handle;
        }

        public override void Dispose()
        {
            _handle = new AudioClipHandle();
        }
    }
}
