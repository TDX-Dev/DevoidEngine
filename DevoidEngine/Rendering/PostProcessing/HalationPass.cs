//using DevoidEngine.Core;
//using DevoidEngine.Rendering;
//using DevoidEngine.Rendering.PostProcessing;
//using DevoidGPU;
//using System.Numerics;
//using System.Runtime.CompilerServices;

//namespace DevoidEngine.Rendering.PostProcessing
//{
//    public class HalationPass : PostProcessPass
//    {
//        private struct HalationShaderData
//        {
//            public float Intensity;
//            public float Threshold;
//            public Vector2 Padding;
//            public Vector3 Color;
//        }

//        public int MipCount { get; set; } = 5;
//        public float Radius { get; set; } = 0.7f;
//        public float Intensity { get; set; } = 0.35f;
//        public float Threshold { get; set; } = 1.0f;
//        public Vector3 Color { get; set; } = new(1.0f, 0.25f, 0.05f);

//        private readonly MaterialInstance halationMaterial;
//        private readonly MipBlurUtility blurUtility;
//        private readonly RenderTarget target;
//        private readonly UniformBuffer shaderDataBuffer;
//        private readonly Sampler sampler;

//        public HalationPass()
//        {
//            string shaderPath = Path.Combine(
//                Engine.BasePath,
//                "Content/DevoidShaderDescriptors/halation_pass.dsd");

//            string downsamplePath = Path.Combine(
//                Engine.BasePath,
//                "Content/DevoidShaderDescriptors/bloom_downsample_pass.dsd");

//            string upsamplePath = Path.Combine(
//                Engine.BasePath,
//                "Content/DevoidShaderDescriptors/bloom_upsample_pass.dsd");

//            halationMaterial = new MaterialInstance(
//                new Material(
//                    Shader.FromDescriptorFile(
//                        Engine.GraphicsDevice,
//                        shaderPath)));

//            MaterialInstance downsampleMaterial = new MaterialInstance(
//                new Material(
//                    Shader.FromDescriptorFile(
//                        Engine.GraphicsDevice,
//                        downsamplePath)));

//            MaterialInstance upsampleMaterial = new MaterialInstance(
//                new Material(
//                    Shader.FromDescriptorFile(
//                        Engine.GraphicsDevice,
//                        upsamplePath)));

//            blurUtility = new MipBlurUtility(
//                downsampleMaterial,
//                upsampleMaterial);

//            target = RenderTarget.Create(1);

//            shaderDataBuffer = new UniformBuffer(
//                Engine.GraphicsDevice,
//                ResourceUsage.Dynamic,
//                (uint)Unsafe.SizeOf<HalationShaderData>());

//            sampler = Sampler.Create(
//                new SamplerDescription
//                {
//                    AddressU = WrapMode.ClampToEdge,
//                    AddressV = WrapMode.ClampToEdge,
//                    AddressW = WrapMode.ClampToEdge,
//                    MagFilter = FilterMode.Linear,
//                    MinFilter = FilterMode.Linear,
//                    MipFilter = FilterMode.Linear,
//                    MinLOD = 0f,
//                    MaxLOD = float.MaxValue,
//                    MaxAnisotropy = 1
//                });
//        }

//        public override void Setup()
//        {
//            Read("SceneColor");
//            Write("Halation");
//        }

//        public override void Execute(PostProcessContext ctx)
//        {
//            Texture scene = ctx.GetTexture("SceneColor");

//            Texture blurredScene = blurUtility.Process(ctx, scene, MipCount, Radius, BloomUtility.MipMode.Normal, "PP_HALATION");

//            RenderHalation(ctx, scene, blurredScene);
//        }

//        private void RenderHalation(PostProcessContext ctx, Texture scene, Texture blurredScene)
//        {
//            TextureDescription desc = scene.GPU.Description;

//            BeginPass(
//                ctx,
//                desc.Width,
//                desc.Height);

//            halationMaterial.SetTexture(
//                "SCENE_TEXTURE",
//                scene);

//            halationMaterial.SetTexture(
//                "BLURRED_TEXTURE",
//                blurredScene);

//            halationMaterial.SetSampler(
//                "SCENE_TEXTURESampler",
//                sampler);

//            halationMaterial.SetSampler(
//                "BLURRED_TEXTURESampler",
//                sampler);

//            HalationShaderData data = new()
//            {
//                Intensity = Intensity,
//                Threshold = Threshold,
//                Color = Color,
//                Padding = Vector2.Zero
//            };

//            shaderDataBuffer.Update(new ReadOnlySpan<HalationShaderData>(ref data));

//            halationMaterial.SetUniformBuffer("HalationShaderData", shaderDataBuffer);

//            ctx.RenderContext.RenderToScreen(halationMaterial);

//            EndPass(ctx);
//        }

//        private void BeginPass(
//            PostProcessContext ctx,
//            int width,
//            int height)
//        {
//            ctx.RenderContext.Viewport.Push(
//                new Viewport(
//                    0,
//                    0,
//                    width,
//                    height));

//            target.AttachColorTexture(
//                0,
//                ctx.RenderContext.Resources.GetOrCreateTexture(
//                    "PP_HALATION",
//                    new TextureDescription
//                    {
//                        Width = width,
//                        Height = height,
//                        Format = ctx.GetTexture("SceneColor").GPU.Description.Format,
//                        MipLevels = 1
//                    }));

//            ctx.RenderContext.SetRenderTarget(target);

//            ctx.RenderContext.Clear(
//                ClearFlags.Color,
//                Vector4.Zero);
//        }

//        private void EndPass(PostProcessContext ctx)
//        {
//            ctx.RenderContext.Viewport.Pop();
//        }
//    }
//}