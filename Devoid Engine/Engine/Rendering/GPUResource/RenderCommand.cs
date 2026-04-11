using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Rendering.GPUResource
{
    public abstract class RenderCommand
    {
        internal Action<RenderCommand>? ReturnToPool;

        public abstract void Execute();
        internal abstract void Reset();

        internal void Release()
        {
            ReturnToPool?.Invoke(this);
        }
    }

    public class CreateVertexBufferCommand : RenderCommand
    {
        public VertexBufferManager Manager = null!;
        public VertexBufferHandle Handle;
        public BufferUsage Usage;
        public VertexInfo VertexInfo = null!;
        public int VertexCount;

        public override void Execute()
        {
            Manager._vertexBuffers[Handle.Id] =
                Renderer.GraphicsDevice.BufferFactory.CreateVertexBuffer(
                    Usage,
                    VertexInfo,
                    VertexCount
                );
        }

        internal override void Reset()
        {
            Handle = default;
            VertexCount = 0;
        }
    }

    class SetVertexBufferDataCommand<T> : RenderCommand where T : struct
    {
        public VertexBufferManager Manager = null!;
        public VertexBufferHandle Handle;
        public T[] Data = null!;

        public override void Execute()
        {
            Manager._vertexBuffers[Handle.Id].SetData<T>(Data);
        }

        internal override void Reset()
        {
            Manager = null!;
            Handle = default;
            Data = null!;
        }
    }

    public class BindVertexBufferCommand : RenderCommand
    {
        public VertexBufferManager Manager = null!;
        public VertexBufferHandle Handle;
        public int Slot;
        public int Offset;

        public override void Execute()
        {
            if (Manager._vertexBuffers.TryGetValue(Handle.Id, out var vb))
                vb.Bind(Slot, Offset);
        }

        internal override void Reset()
        {
            Handle = default;
            Slot = 0;
            Offset = 0;
        }
    }

    public class DeleteVertexBufferCommand : RenderCommand
    {
        public VertexBufferManager Manager = null!;
        public VertexBufferHandle Handle;

        public override void Execute()
        {
            if (Manager._vertexBuffers.TryGetValue(Handle.Id, out var vb))
            {
                vb.Dispose();
                Manager._vertexBuffers.Remove(Handle.Id);
            }
        }

        internal override void Reset()
        {
            Handle = default;
        }
    }

    class CreateFramebufferCommand : RenderCommand
    {
        public FramebufferManager Manager = null!;
        public FrameBufferHandle Handle;

        public override void Execute()
        {
            Manager._frameBuffers[Handle.Id] =
                Renderer.GraphicsDevice.BufferFactory.CreateFramebuffer();
        }

        internal override void Reset()
        {
            Handle = default;
        }
    }

    class BindFramebufferCommand : RenderCommand
    {
        public FramebufferManager Manager = null!;
        public FrameBufferHandle Handle;

        public override void Execute()
        {
            Renderer.GraphicsDevice.BindFramebuffer(
                Manager._frameBuffers[Handle.Id]
            );
        }

        internal override void Reset()
        {
            Handle = default;
        }
    }

    class AttachRenderTextureCommand : RenderCommand
    {
        public FramebufferManager Manager = null!;
        public FrameBufferHandle Handle;
        public TextureHandle Texture;
        public int Index;

        public override void Execute()
        {
            Manager._frameBuffers[Handle.Id].AddColorAttachment(
                (ITexture2D)Renderer.ResourceManager.TextureManager.GetDeviceTexture(Texture),
                Index
            );
        }

        internal override void Reset()
        {
            Handle = default;
            Texture = default;
            Index = 0;
        }
    }

    class AttachRenderTextureCubeCommand : RenderCommand
    {
        public FramebufferManager Manager = null!;
        public FrameBufferHandle Handle;
        public TextureHandle Texture;
        public CubeFace FaceIndex;
        public int MipLevel;
        public int Index;

        public override void Execute()
        {
            Manager._frameBuffers[Handle.Id].AddColorAttachment(
                (ITextureCube)Renderer.ResourceManager.TextureManager.GetDeviceTexture(Texture),
                FaceIndex,
                MipLevel,
                Index
            );
        }

        internal override void Reset()
        {
            Handle = default;
            Texture = default;
            FaceIndex = default;
            MipLevel = 0;
            Index = 0;
        }
    }

    class AttachDepthTextureCommand : RenderCommand
    {
        public FramebufferManager Manager = null!;
        public FrameBufferHandle Handle;
        public TextureHandle Texture;

        public override void Execute()
        {
            Manager._frameBuffers[Handle.Id].AddDepthAttachment(
                (ITexture2D)Renderer.ResourceManager.TextureManager.GetDeviceTexture(Texture)
            );
        }

        internal override void Reset()
        {
            Handle = default;
            Texture = default;
        }
    }

    class ClearFramebufferColorCommand : RenderCommand
    {
        public FramebufferManager Manager = null!;
        public FrameBufferHandle Handle;
        public Vector4 Color;

        public override void Execute()
        {
            Manager._frameBuffers[Handle.Id].ClearColor(Color);
        }

        internal override void Reset()
        {
            Handle = default;
            Color = default;
        }
    }

    class ClearFramebufferDepthCommand : RenderCommand
    {
        public FramebufferManager Manager = null!;
        public FrameBufferHandle Handle;
        public int Value;

        public override void Execute()
        {
            Manager._frameBuffers[Handle.Id].ClearDepth(Value);
        }

        internal override void Reset()
        {
            Handle = default;
            Value = 0;
        }
    }

    class CreateIndexBufferCommand : RenderCommand
    {
        public IndexBufferManager Manager = null!;
        public IndexBufferHandle Handle;
        public BufferUsage Usage;
        public int IndexCount;

        public override void Execute()
        {
            Manager._indexBuffers[Handle.Id] =
                Renderer.GraphicsDevice.BufferFactory.CreateIndexBuffer(
                    IndexCount,
                    Usage
                );
        }

        internal override void Reset()
        {
            Handle = default;
            Usage = default;
            IndexCount = 0;
        }
    }

    class BindIndexBufferCommand : RenderCommand
    {
        public IndexBufferManager Manager = null!;
        public IndexBufferHandle Handle;

        public override void Execute()
        {
            Manager._indexBuffers[Handle.Id].Bind();
        }

        internal override void Reset()
        {
            Handle = default;
        }
    }

    class SetIndexBufferDataCommand : RenderCommand
    {
        public IndexBufferManager Manager = null!;
        public IndexBufferHandle Handle;
        public int[] Data = null!;

        public override void Execute()
        {
            Manager._indexBuffers[Handle.Id].SetData(Data);
        }

        internal override void Reset()
        {
            Handle = default;
            Data = [];
        }
    }

    class DeleteIndexBufferCommand : RenderCommand
    {
        public IndexBufferManager Manager = null!;
        public IndexBufferHandle Handle;

        public override void Execute()
        {
            if (Manager._indexBuffers.TryGetValue(Handle.Id, out var ib))
            {
                ib.Dispose();
                Manager._indexBuffers.Remove(Handle.Id);
            }
        }

        internal override void Reset()
        {
            Handle = default;
        }
    }

    class CreateSamplerCommand : RenderCommand
    {
        public SamplerManager Manager = null!;
        public SamplerHandle Handle;
        public SamplerDescription Description = null!;

        public override void Execute()
        {
            Manager._samplers[Handle.Id] =
                Renderer.GraphicsDevice.CreateSampler(Description);
        }

        internal override void Reset()
        {
            Manager = null!;
            Handle = default;
            Description = null!;
        }
    }

    class DeleteSamplerCommand : RenderCommand
    {
        public SamplerManager Manager = null!;
        public SamplerHandle Handle;

        public override void Execute()
        {
            if (Manager._samplers.TryGetValue(Handle.Id, out var sampler))
            {
                sampler.Dispose();
                Manager._samplers.Remove(Handle.Id);
            }
        }

        internal override void Reset()
        {
            Manager = null!;
            Handle = default;
        }
    }

    class CreateTextureCommand : RenderCommand
    {
        public TextureManager Manager = null!;
        public TextureHandle Handle;
        public TextureDescription Description;

        public override void Execute()
        {
            Manager._textures[Handle.Id] =
                Renderer.GraphicsDevice.TextureFactory.CreateTexture(Description);
        }

        internal override void Reset()
        {
            Manager = null!;
            Handle = default;
            Description = default;
        }
    }

    class UploadTextureData2DCommand : RenderCommand
    {
        public TextureManager Manager = null!;
        public TextureHandle Handle;
        public byte[] Data = null!;

        public override void Execute()
        {
            ITexture2D textureInternal = (ITexture2D)Manager._textures[Handle.Id];
            textureInternal.SetData(Data);
        }

        internal override void Reset()
        {
            Manager = null!;
            Handle = default;
            Data = null!;
        }
    }

    class UploadTextureDataCubeCommand : RenderCommand
    {
        public TextureManager Manager = null!;
        public TextureHandle Handle;
        public CubeFace Face;
        public byte[] Data = null!;

        public override void Execute()
        {
            ITextureCube textureInternal = (ITextureCube)Manager._textures[Handle.Id];
            textureInternal.SetData(Face, Data);
        }

        internal override void Reset()
        {
            Manager = null!;
            Handle = default;
            Face = default;
            Data = null!;
        }
    }

    class GenerateMipmapsCommand : RenderCommand
    {
        public TextureManager Manager = null!;
        public TextureHandle Handle;

        public override void Execute()
        {
            Manager._textures[Handle.Id].GenerateMipmaps();
        }

        internal override void Reset()
        {
            Manager = null!;
            Handle = default;
        }
    }

    class DeleteTextureCommand : RenderCommand
    {
        public TextureManager Manager = null!;
        public TextureHandle Handle;

        public override void Execute()
        {
            if (Manager._textures.TryGetValue(Handle.Id, out var tex))
            {
                tex.Dispose();
                Manager._textures.Remove(Handle.Id);
            }
        }

        internal override void Reset()
        {
            Manager = null!;
            Handle = default;
        }
    }

    class DestroyFramebufferCommand : RenderCommand
    {
        public FramebufferManager Manager = null!;
        public FrameBufferHandle Handle;

        public override void Execute()
        {
            if (Manager._frameBuffers.TryGetValue(Handle.Id, out var fb))
            {
                fb.Dispose();
                Manager._frameBuffers.Remove(Handle.Id);
            }
        }

        internal override void Reset()
        {
            Manager = null!;
            Handle = default;
        }
    }
}
