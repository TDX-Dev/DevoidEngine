using DevoidGPU;
using SharpFont;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Rendering
{
    public static class Graphics
    {
        public static int mainThreadID = -1;
        public static volatile bool MainThreadStarted = false;

        private static uint _nextTextureID = 0;
        private static uint _nextSamplerID = 0;

        // Replace possibly with its own IGPUCommand
        private static ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();

        public static GraphicsFence CreateFence()
        {
            var fence = new GraphicsFence();

            _queue.Enqueue(() =>
            {
                fence.Signal();
            });

            return fence;
        }

        public static TextureHandle CreateTexture(TextureDescription textureDescription)
        {
            TextureHandle textureHandle = new TextureHandle(++_nextTextureID);

            Enqueue(() =>
            {
                RenderBase._textures[textureHandle.Id]
                    = Renderer.graphicsDevice.TextureFactory.CreateTexture(
                        textureDescription
                    );
            });

            return textureHandle;
        }

        public static ITexture GetDeviceTexture(TextureHandle handle)
        {
            return RenderBase._textures[handle.Id];
        }

        public static void UploadTextureData2D(TextureHandle handle, byte[] data)
        {
            ITexture2D textureInternal = (ITexture2D)RenderBase._textures[handle.Id];

            Enqueue(() =>
            {
                textureInternal.SetData(data);
            });
        }

        public static void GenerateMipmaps(TextureHandle handle)
        {
            ITexture textureInternal = RenderBase._textures[handle.Id];
            Enqueue(() =>
            {
                textureInternal.GenerateMipmaps();
            });
        }

        public static void DeleteTexture(TextureHandle handle)
        {
            ITexture textureInternal = RenderBase._textures[handle.Id];
            Enqueue(() =>
            {
                textureInternal.Dispose();
            });
        }

        public static void BindTexture(TextureHandle handle, int slot, ShaderStage stage = ShaderStage.Fragment, BindMode mode = BindMode.ReadOnly)
        {
            if (mode == BindMode.ReadOnly)
                RenderBase._textures[handle.Id].Bind(slot);
            else
                RenderBase._textures[handle.Id].BindMutable(slot);

        }

        public static void UnBindTexture(TextureHandle handle, int slot, ShaderStage stage = ShaderStage.Fragment, BindMode mode = BindMode.ReadOnly)
        {
            RenderBase._textures[handle.Id].UnBind(slot);
        }

        public static void BindSampler(SamplerHandle handle, int slot)
        {
            RenderBase._samplers[handle.Id].Bind(slot);
        }

        public static SamplerHandle CreateSampler(SamplerDescription samplerDescription)
        {
            SamplerHandle samplerHandle = new SamplerHandle(++_nextSamplerID);

            Enqueue(() =>
            {


                RenderBase._samplers[samplerHandle.Id]
                    = Renderer.graphicsDevice.CreateSampler(
                        samplerDescription
                    );
            });

            return samplerHandle;
        }

        public static void Enqueue(Action action)
        {
            if (Thread.CurrentThread.ManagedThreadId == mainThreadID)
            {
                action();
                return;
            }
            _queue.Enqueue(action);
        }

        public static void Execute()
        {
            while (_queue.TryDequeue(out var action))
                action();
        }


    }

    public readonly struct SamplerHandle
    {
        public readonly uint Id;

        internal SamplerHandle(uint id)
        {
            Id = id;
        }
    }


    public readonly struct TextureHandle
    {
        public readonly uint Id;

        internal TextureHandle(uint id)
        {
            Id = id;
        }
    }
}
