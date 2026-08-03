//using DevoidEngine.Core;
//using DevoidGPU;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Numerics;
//using System.Runtime.CompilerServices;
//using System.Text;
//using System.Threading.Tasks;

//namespace DevoidEngine.Rendering.PostProcessing
//{
//    struct BloomMipShaderData
//    {
//        public Vector2 MipSize;
//        public int MipLevel;
//        public float FilterRadius;
//    }

//    internal class BloomPass : PostProcessPass
//    {
//        public int BloomMipCount { get; set; } = 8;

//        public float BloomFilterRadius { get; set; }

//        private readonly MaterialInstance downsampleMaterial;
//        private readonly MaterialInstance upsampleMaterial;

//        private readonly RenderTarget renderTarget;

//        private readonly UniformBuffer mipShaderDataBuffer;

//        public BloomPass()
//        {
//            downsampleMaterial = new MaterialInstance(
//                new Material(
//                    Shader.FromDescriptorFile(
//                        Engine.GraphicsDevice,
//                        "Content/DevoidShaderDescriptors/bloom_downsample.dsd")));

//            upsampleMaterial = new MaterialInstance(
//                new Material(
//                    Shader.FromDescriptorFile(
//                        Engine.GraphicsDevice,
//                        "Content/DevoidShaderDescriptors/bloom_upsample.dsd")));

//            renderTarget = RenderTarget.Create(1);

//            mipShaderDataBuffer = UniformBuffer.Create(
//                ResourceUsage.Dynamic,
//                (uint)Unsafe.SizeOf<BloomMipShaderData>());
//        }

//        public override void Setup()
//        {
//            Read("SceneColor");
//            Write("Bloom");
//        }

//        public override void Execute(PostProcessContext ctx)
//        {
//            RenderDownsamples(ctx);
//            RenderUpsamples(ctx);

//            Texture bloom =
//                ctx.RenderContext.Resources.GetOrCreateTexture("BloomMip0");

//            ctx.SetTexture("Bloom", bloom);
//        }

//        private void RenderDownsamples(PostProcessContext ctx)
//        {
//            int width = ctx.RenderContext.Viewport.Width;
//            int height = ctx.RenderContext.Viewport.Height;

//            Texture source = ctx.GetTexture("SceneColor");

//            for (int mip = 0; mip < BloomMipCount; mip++)
//            {
//                width = Math.Max(1, width / 2);
//                height = Math.Max(1, height / 2);

//                TextureDescription desc = new()
//                {
//                    Width = width,
//                    Height = height,
//                    Depth = 1,
//                    ArraySize = 1,
//                    MipLevels = 1,
//                    Dimension = TextureDimension.Texture2D,
//                    Format = TextureFormat.RGBA16_Float,
//                    Samples = new TextureSampleDescription(1, 0),
//                    Usage = TextureUsage.RenderTarget | TextureUsage.ShaderResource
//                };

//                Texture destination =
//                    ctx.RenderContext.Resources.GetOrCreateTexture(
//                        $"BloomMip{mip}",
//                        desc);

//                ...
//    }
//        }
//    }
//}
