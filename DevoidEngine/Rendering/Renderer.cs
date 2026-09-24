using DevoidEngine.Core;
using DevoidEngine.Rendering.PostProcessing;
using DevoidEngine.Rendering.ProbeGI;
using DevoidEngine.Util;
using DevoidGPU;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace DevoidEngine.Rendering
{
    public struct ViewportRect
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
    }

    public struct ScissorRect
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
    }
    public enum RenderTechnique
    {
        Forward,
        Clustered
    }

    public struct RendererConfig
    {
        public RenderTechnique Technique;
    }

    public sealed class Renderer : IDisposable
    {
        public IRenderTechnique? ActiveTechnique { get; set; }

        public const uint MAX_POINT_LIGHTS = 100;
        public const uint MAX_SPOT_LIGHTS = 15;
        public const uint MAX_DIRECTIONAL_LIGHTS = 2;

        public ShaderLibrary ShaderLibrary { get; set; } = null!;
        public RenderAPI API { get; private set; } = null!;
        public Texture BlueNoiseTexture { get; private set; } = null!;
        public Material DefaultMaterial { get; private set; } = null!;
        public Material NullMaterial { get; private set; } = null!;
        public MaterialInstance NullMaterialInstance { get; private set; } = null!;
        public Shader DefaultShader { get; private set; } = null!;
        public Shader InformationShader { get; private set; } = null!;
        public Shader DownsampleShader { get; private set; } = null!;
        public MaterialInstance InformationMaterialInstance { get; private set; } = null!;
        public MaterialInstance DownsampleMaterialInstance { get; private set; } = null!;
        public Shader VBAOShader {  get; private set; } = null!;
        public MaterialInstance VBAOMaterialInstance { get; private set; } = null!;
        public RenderTarget ViewportBlitTarget { get; private set; } = null!;
        public RenderTarget UIRenderTarget { get; private set; } = null!;
        public RenderTarget DebugRenderTarget { get; private set; } = null!;
        public RenderTarget InfoRenderTarget { get; private set; } = null!;
        public RenderTarget VBAORenderTarget { get; private set; } = null!;
        public RenderTarget BlurRenderTarget { get; private set; } = null!;
        public UniformBuffer CameraBuffer { get; private set; } = null!;
        public UniformBuffer SceneBuffer { get; private set; } = null!;
        public UniformBuffer PerObjectBuffer { get; private set; } = null!;
        public UniformBuffer PerFrameBuffer { get; private set; } = null!;
        public SkyRenderer SkyRenderer { get; private set; } = null!;
        public GizmoRenderer GizmoRenderer { get; private set; } = null!;
        public PostProcessor PostProcessor { get; private set; } = null!;
        public ProbeGISystem ProbeGISystem { get; private set; } = null!;
        public ProbeGISettings ProbeGISettings
        {
            get => probeGISettings;
            set
            {
                probeGISettings = value;
                probeGISettingsChanged = true;
            }
        }

        public EnvironmentLighting Environment => SkyRenderer.Environment;

        private IDescriptorLayout PerCameraDescriptorLayout = null!;
        private IDescriptorSet PerCameraDescriptor = null!;

        private IDescriptorLayout PerObjectDescriptorLayout = null!;
        private IDescriptorSet PerObjectDescriptor = null!;

        private IDescriptorLayout PerFrameDescriptorLayout = null!;
        private IDescriptorSet PerFrameDescriptor = null!;

        private Stack<ViewportRect> viewportStack = null!;
        private ViewportRect currentViewport;

        private Stack<ScissorRect> scissorStack = null!;
        private ScissorRect currentScissor;

        private RenderView renderView;

        private ProbeGISettings probeGISettings;

        private ShaderStorageBuffer<GPUPointLight> PointLightBuffer = null!;
        private ShaderStorageBuffer<GPUSpotLight> SpotLightBuffer = null!;
        private ShaderStorageBuffer<GPUDirectionalLight> DirectionalLightBuffer = null!;

        private Dictionary<Viewport, RenderResourceCache> RenderResources = null!;

        private Pool<RenderMeshData> uiRenderDataPool = null!;
        private Pool<RenderMeshData> gizmoRenderDataPool = null!;
        private readonly List<RenderMeshData> uiRenderCache = [];
        private readonly List<RenderMeshData> gizmoRenderCache = [];
        
        private Sampler DownsampleSampler = null!;

        private readonly Queue<Action<ICommandList>> pendingGpuCommands = [];

        private bool probeGISettingsChanged = true;

        public void PushViewport(ICommandList cmd, ViewportRect viewport)
        {
            viewportStack.Push(currentViewport);
            currentViewport = viewport;

            cmd.SetViewport(
                viewport.X,
                viewport.Y,
                viewport.Width,
                viewport.Height);
        }

        public void PopViewport(ICommandList cmd, bool setPrevious = true)
        {
            if (viewportStack.Count == 0)
                return;
            currentViewport = viewportStack.Pop();

            if (setPrevious)
            {
                cmd.SetViewport(
                currentViewport.X,
                currentViewport.Y,
                currentViewport.Width,
                currentViewport.Height);
            }
        }
        public void PushScissor(ICommandList cmd, ScissorRect scissor)
        {
            scissorStack.Push(currentScissor);
            currentScissor = scissor;

            cmd.SetScissor(
                scissor.X,
                scissor.Y,
                scissor.Width,
                scissor.Height);
        }

        public void PopScissor(ICommandList cmd, bool setPrevious = true)
        {
            if (scissorStack.Count == 0)
                return;

            currentScissor = scissorStack.Pop();

            if (setPrevious)
            {
                cmd.SetScissor(
                    currentScissor.X,
                    currentScissor.Y,
                    currentScissor.Width,
                    currentScissor.Height);
            }
        }

        public void Initialize(RendererConfig config)
        {
            if (Engine.GraphicsDevice == null)
                throw new Exception("Graphics device not initialized yet.");

            viewportStack = new Stack<ViewportRect>();
            scissorStack = new Stack<ScissorRect>();
            uiRenderDataPool = new Pool<RenderMeshData>();
            gizmoRenderDataPool = new Pool<RenderMeshData>();
            RenderResources = [];

            ActiveTechnique = config.Technique switch
            {
                RenderTechnique.Forward => new ForwardRenderTechnique(),
                _ => throw new NotImplementedException(nameof(config.Technique) + " is not implemented."),
            };

            ShaderLibrary = new ShaderLibrary();

            DefaultShader = Shader.FromDescriptorFile(Engine.GraphicsDevice, Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/pbr_mat.dsd"));
            DefaultMaterial = new Material(DefaultShader);

            Shader NullShader = Shader.FromDescriptorFile(Engine.GraphicsDevice, Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/null_mat.dsd"));
            NullMaterial = new Material(NullShader);
            NullMaterialInstance = new MaterialInstance(NullMaterial);


            InformationShader = Shader.FromDescriptorFile(Engine.GraphicsDevice, Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/information_pass.dsd"));
            InformationMaterialInstance = new MaterialInstance(new Material(InformationShader));

            DownsampleShader = Shader.FromDescriptorFile(Engine.GraphicsDevice, Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/bloom_downsample_pass.dsd"));
            DownsampleMaterialInstance = new MaterialInstance(new Material(DownsampleShader));

            VBAOShader = Shader.FromDescriptorFile(Engine.GraphicsDevice, Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/gtvbao.dsd"));

            BlueNoiseTexture = Texture.CreateFromImage2D(TextureUtil.LoadImage("Content/Noise/LDR_RG01_0.png"), TextureUsage.ShaderResource, TextureFormat.RG8_UNorm);

            VBAOMaterialInstance = new MaterialInstance(new Material(VBAOShader));

            PerCameraDescriptorLayout = Engine.GraphicsDevice.CreateDescriptorLayout([
                new() {
                    Binding = 0,
                    Stages = DevoidGPU.ShaderStage.Vertex | DevoidGPU.ShaderStage.Fragment,
                    Type = DescriptorType.UniformBuffer
                },
                new() {
                    Binding = 3,
                    Stages = DevoidGPU.ShaderStage.Fragment,
                    Type = DescriptorType.UniformBuffer
                },
                new() {
                    Binding = 10,
                    Stages = DevoidGPU.ShaderStage.Fragment,
                    Type = DescriptorType.StorageBuffer
                },
                new() {
                    Binding = 11,
                    Stages = DevoidGPU.ShaderStage.Fragment,
                    Type = DescriptorType.StorageBuffer
                },
                new() {
                    Binding = 12,
                    Stages = DevoidGPU.ShaderStage.Fragment,
                    Type = DescriptorType.StorageBuffer
                },
                new() {
                    Binding = 15,
                    Stages = DevoidGPU.ShaderStage.Fragment,
                    Type = DescriptorType.Texture
                },
                new() {
                    Binding = 16,
                    Stages = DevoidGPU.ShaderStage.Fragment,
                    Type = DescriptorType.Texture
                },
                new() {
                    Binding = 17,
                    Stages = DevoidGPU.ShaderStage.Fragment,
                    Type = DescriptorType.StorageBuffer
                },
                new() {
                    Binding = 18,
                    Stages = DevoidGPU.ShaderStage.Fragment,
                    Type = DescriptorType.Texture
                },
                new() {
                    Binding = 19,
                    Stages = DevoidGPU.ShaderStage.Fragment,
                    Type = DescriptorType.Texture
                },
                new() {
                    Binding = 20,
                    Stages = DevoidGPU.ShaderStage.Fragment,
                    Type = DescriptorType.StorageBuffer
                },
                new() {
                    Binding = 6,
                    Stages = DevoidGPU.ShaderStage.Fragment,
                    Type = DescriptorType.UniformBuffer
                }
            ]);

            PerCameraDescriptor = Engine.GraphicsDevice.CreateDescriptorSet(PerCameraDescriptorLayout);

            PerObjectDescriptorLayout = Engine.GraphicsDevice.CreateDescriptorLayout([
                new() {
                    Binding = 1,
                    Stages = DevoidGPU.ShaderStage.Vertex | DevoidGPU.ShaderStage.Fragment,
                    Type = DescriptorType.UniformBuffer
                }
            ]);

            PerObjectDescriptor = Engine.GraphicsDevice.CreateDescriptorSet(PerObjectDescriptorLayout);

            PerFrameDescriptorLayout = Engine.GraphicsDevice.CreateDescriptorLayout([
                new() {
                    Binding = 2,
                    Stages =  DevoidGPU.ShaderStage.Vertex | DevoidGPU.ShaderStage.Fragment,
                    Type = DescriptorType.UniformBuffer
                }
            ]);

            PerFrameDescriptor = Engine.GraphicsDevice.CreateDescriptorSet(PerFrameDescriptorLayout);

            CameraBuffer = UniformBuffer.Create(ResourceUsage.Dynamic, (uint)Unsafe.SizeOf<CameraData>());
            PerObjectBuffer = UniformBuffer.Create(ResourceUsage.Dynamic, (uint)Unsafe.SizeOf<MeshRenderData>());
            SceneBuffer = UniformBuffer.Create(ResourceUsage.Dynamic, (uint)Unsafe.SizeOf<SceneData>());
            PerFrameBuffer = UniformBuffer.Create(ResourceUsage.Dynamic, (uint)Unsafe.SizeOf<PerFrameData>());

            PerCameraDescriptor.SetUniformBuffer(0, CameraBuffer.GPU);
            PerCameraDescriptor.SetUniformBuffer(3, SceneBuffer.GPU);
            PerObjectDescriptor.SetUniformBuffer(1, PerObjectBuffer.GPU);

            PerFrameDescriptor.SetUniformBuffer(2, PerFrameBuffer.GPU);

            API = new RenderAPI();

            ViewportBlitTarget = RenderTarget.Create(1);
            UIRenderTarget = RenderTarget.Create(1);
            DebugRenderTarget = RenderTarget.Create(1);
            InfoRenderTarget = RenderTarget.Create(3);
            VBAORenderTarget = RenderTarget.Create(1);
            BlurRenderTarget = RenderTarget.Create(1);

            renderView = new RenderView();

            PointLightBuffer = ShaderStorageBuffer<GPUPointLight>.Create(ResourceUsage.Dynamic, Renderer.MAX_POINT_LIGHTS);
            SpotLightBuffer = ShaderStorageBuffer<GPUSpotLight>.Create(ResourceUsage.Dynamic, Renderer.MAX_SPOT_LIGHTS);
            DirectionalLightBuffer = ShaderStorageBuffer<GPUDirectionalLight>.Create(ResourceUsage.Dynamic, Renderer.MAX_DIRECTIONAL_LIGHTS);

            SkyRenderer = new SkyRenderer();
            GizmoRenderer = new GizmoRenderer();
            GizmoRenderer.Initialize(Engine.GraphicsDevice, Engine.BasePath);

            DownsampleSampler = Sampler.Create(new SamplerDescription
            {
                AddressU = WrapMode.ClampToBorder,
                AddressV = WrapMode.ClampToBorder,
                AddressW = WrapMode.ClampToBorder,
                MagFilter = FilterMode.Linear,
                MinFilter = FilterMode.Linear,
                MipFilter = FilterMode.Linear,
                MinLOD = 0f,
                MaxLOD = float.MaxValue,
                MaxAnisotropy = 1
            });

            DownsampleMaterialInstance.SetSampler("INPUT_TEXTURESampler", DownsampleSampler);

            PostProcessor = new PostProcessor();


            PostProcessor.AddPass(new TonemapPass());
            PostProcessor.AddPass(new BloomPass());
            PostProcessor.AddPass(new AnamorphicBloomPass());

            ProbeGISystem = new ProbeGISystem();
            probeGISettings = new ProbeGISettings();

            ActiveTechnique.Initialize();

        }
        public void PrepareGlobalFrame(ICommandList cmd)
        {
            if (probeGISettingsChanged)
            {
                ProbeGISystem.ResolveSettings(ref probeGISettings);
                ProbeGISystem.RecreateAndClear(cmd, probeGISettings);
                probeGISettingsChanged = false;
            }

            UpdatePerFrameData(new PerFrameData()
            {
                FrameIndex = (int)Engine.Instance.FrameCount,
                ProbeGISettings = probeGISettings,
            });

            if (Engine.Instance.FrameCount == 0)
                ProbeGISystem.RecreateAndClear(cmd, probeGISettings);
        }
        public void Render(ICommandList cmd, Viewport viewport)
        {

            //ViewportBlitTarget.SetColorAttachment(0, viewport.OutputTexture!);

            //cmd.SetDescriptorSet(3, PerFrameDescriptor);

            //cmd.SetFramebuffer(ViewportBlitTarget.GPU);
            //cmd.ClearColor(0, Colors.Transparent);

            //if (viewport.ActiveCamera == null || ActiveTechnique == null)
            //    return;

            //ExecutePendingGPUCommands(cmd);

            //PopViewport(cmd);
            //PushViewport(cmd, new ViewportRect()
            //{
            //    Width = viewport.Width,
            //    Height = viewport.Height,
            //    X = 0,
            //    Y = 0,
            //});


            //Camera camera = viewport.ActiveCamera;
            //camera.UpdateProjectionMatrix(((float)viewport.Width) / viewport.Height);

            //RenderResourceCache viewportResources = RenderResources[viewport];

            //RenderContext context = new()
            //{
            //    Viewport = viewport,
            //    Camera = camera,
            //    CommandList = cmd,
            //    Renderer = this,
            //    Resources = viewportResources
            //};

            //SkyRenderer.Render(context);

            //renderView.Clear();
            //viewport.TargetScene.World.BuildView(camera, ref renderView);
            //UpdateCameraBuffer(camera.GetCameraData(new Vector2(viewport.Width, viewport.Height)));
            //UpdateSceneData(renderView);
            //UpdateLights(renderView);

            //RenderInformationPass(ref context, renderView);

            //RenderVBAOPass(ref context);

            //RenderTarget activeTechniqueTarget = ActiveTechnique.Render(context, renderView);

            //context.SceneColor = activeTechniqueTarget.ColorTextures[0]!;

            //RenderSceneBlurPass(cmd, ref context);

            //Texture finalColor = PostProcessor.Run(this, context, activeTechniqueTarget.ColorTextures[0]!, viewport.TargetScene.World.GetPostProcessSettings(viewport.ActiveCamera) ?? viewport.TargetScene.WorldEnvironmentNode?.PostProcessSettings ?? PostProcessSettings.Default);

            //GizmoRenderer.Render(context, viewport, viewportResources);
            ////RenderDebug(cmd, context, viewportResources);

            //ViewportBlitTarget.SetColorAttachment(0, viewport.OutputTexture!);
            //cmd.SetFramebuffer(ViewportBlitTarget.GPU);
            //API.RenderToScreen(cmd, finalColor);
            //API.RenderToScreen(cmd, DebugRenderTarget.ColorTextures[0]!);
            //API.RenderToScreen(cmd, GizmoRenderer.GizmoRenderTarget.ColorTextures[0]!);



            //if (viewport.TryConsumeObjectPickRequest(out ObjectPickRequest request))
            //{
            //    viewport.CompleteObjectPick(new ObjectPickResult(GetIdentifierAtLocation(context, request.Location)));
            //}
        }

        public uint GetIdentifierAtLocation(RenderContext ctx, Vector2 location)
        {
            int x = (int)MathF.Floor(location.X);
            int y = (int)MathF.Floor(location.Y);
            x = Math.Clamp(x, 0, ctx.Viewport.Width - 1);
            y = Math.Clamp(y, 0, ctx.Viewport.Height - 1);

            TextureDescription stagingTextureDescription = new()
            {
                Width = 1,
                Height = 1,
                Depth = 1,
                Format = TextureFormat.R32_UInt,
                Dimension = TextureDimension.Texture2D,
                ArraySize = 1,
                Samples = new TextureSampleDescription(1, 0),
                MipLevels = 1,
                ResourceUsage = ResourceUsage.Staging,
                CPUAccess = CpuAccess.Read
            };

            Texture stagingTexture = ctx.Resources.GetOrCreateTexture("IDENTIFIER_STAGING_TEXTURE", stagingTextureDescription);
            ctx.CommandList.CopyTextureSubresourceRegion(ctx.SceneIdentifier.GPU, stagingTexture.GPU, x, y, 1, 1);
            MappedTexture textureMap = ctx.CommandList.MapTexture(stagingTexture.GPU, MapMode.Read);

            uint identifier;

            unsafe
            {
                identifier = *(uint*)textureMap.Data;
            }

            ctx.CommandList.UnmapTexture(stagingTexture.GPU);
            return identifier;
        }
        public void RenderInformationPass(ref RenderContext context, RenderView view)
        {
            TextureDescription textureNormalDescription = new()
            {
                Width = context.Viewport.Width,
                Height = context.Viewport.Height,
                Depth = 1,
                Format = TextureFormat.RGBA16_Float,
                Dimension = TextureDimension.Texture2D,
                ArraySize = 1,
                Samples = new TextureSampleDescription(1, 0),
                MipLevels = 1,
                Usage = TextureUsage.RenderTarget | TextureUsage.ShaderResource
            };

            TextureDescription textureIdentifierDescription = new()
            {
                Width = context.Viewport.Width,
                Height = context.Viewport.Height,
                Depth = 1,
                Format = TextureFormat.R32_UInt,
                Dimension = TextureDimension.Texture2D,
                ArraySize = 1,
                Samples = new TextureSampleDescription(1, 0),
                MipLevels = 1,
                Usage = TextureUsage.RenderTarget | TextureUsage.ShaderResource,
            };

            TextureDescription textureViewDepthDescription = new()
            {
                Width = context.Viewport.Width,
                Height = context.Viewport.Height,
                Depth = 1,
                Format = TextureFormat.R32_Float,
                Dimension = TextureDimension.Texture2D,
                ArraySize = 1,
                Samples = new TextureSampleDescription(1, 0),
                MipLevels = 1,
                Usage = TextureUsage.RenderTarget | TextureUsage.ShaderResource,
            };

            TextureDescription textureDepthDescription = new()
            {
                Width = context.Viewport.Width,
                Height = context.Viewport.Height,
                Depth = 1,
                Format = TextureFormat.Depth32_Float,
                Dimension = TextureDimension.Texture2D,
                ArraySize = 1,
                Samples = new TextureSampleDescription(1, 0),
                MipLevels = 1,
                Usage = TextureUsage.DepthStencil | TextureUsage.ShaderResource,
            };

            Texture normalTexture = context.Resources.GetOrCreateTexture("RENDERPASSINFO_NORMALS", textureNormalDescription);
            Texture identifierTexture = context.Resources.GetOrCreateTexture("RENDERPASSINFO_IDENTIFIER", textureIdentifierDescription);
            Texture viewDepthTexture = context.Resources.GetOrCreateTexture("RENDERPASSINFO_VIEWDEPTH", textureViewDepthDescription);
            Texture depthTexture = context.Resources.GetOrCreateTexture("RENDERPASSINFO_DEPTH", textureDepthDescription);

            InfoRenderTarget.SetColorAttachment(0, normalTexture);
            InfoRenderTarget.SetColorAttachment(1, identifierTexture);
            InfoRenderTarget.SetColorAttachment(2, viewDepthTexture);
            InfoRenderTarget.SetDepthAttachment(depthTexture);

            context.CommandList.SetFramebuffer(InfoRenderTarget.GPU);

            context.CommandList.ClearColor(0, new Vector4(0, 0, 0, 1));
            context.CommandList.ClearColor(1, new Vector4(0, 0, 0, 1));
            context.CommandList.ClearColor(2, new Vector4(0, 0, 0, 1));
            context.CommandList.ClearDepthStencil(1, 0);



            Execute(context.CommandList, view.Objects, InformationMaterialInstance);

            context.SceneDepth = depthTexture;
            context.SceneViewDepth = viewDepthTexture;
            context.SceneNormal = normalTexture;
            context.SceneIdentifier = identifierTexture;
        }
        public void RenderDebug(ICommandList cmd, RenderContext context, RenderResourceCache resources)
        {
            TextureDescription debugColorTextureDesc = new()
            {
                Width = context.Viewport.Width,
                Height = context.Viewport.Height,
                Depth = 1,
                Format = TextureFormat.RGBA8_UNorm,
                Dimension = TextureDimension.Texture2D,
                ArraySize = 1,
                Samples = new TextureSampleDescription(1, 0),
                MipLevels = 1,
                Usage = TextureUsage.RenderTarget | TextureUsage.ShaderResource
            };

            Texture DebugColorTexture = resources.GetOrCreateTexture("DEBUG_RENDER_COLOR", debugColorTextureDesc);

            DebugRenderTarget.SetColorAttachment(0, DebugColorTexture);
            DebugRenderTarget.SetDepthAttachment(context.SceneDepth);

            cmd.SetFramebuffer(DebugRenderTarget.GPU);
            cmd.ClearColor(0, Colors.Transparent);

            ProbeGISystem.DrawProbeSpheres(cmd, probeGISettings);

        }
        public void RenderVBAOPass(ref RenderContext context)
        {
            TextureDescription textureAODescription = new()
            {
                Width = context.Viewport.Width / 2,
                Height = context.Viewport.Height / 2,
                Depth = 1,
                Format = TextureFormat.R8_UNorm,
                Dimension = TextureDimension.Texture2D,
                ArraySize = 1,
                Samples = new TextureSampleDescription(1, 0),
                MipLevels = 1,
                Usage = TextureUsage.RenderTarget | TextureUsage.ShaderResource
            };

            Texture AOTexture = context.Resources.GetOrCreateTexture("RENDERPASS_VBAO", textureAODescription);

            VBAOMaterialInstance.SetTexture("NormalTexture", context.SceneNormal);
            VBAOMaterialInstance.SetTexture("DepthTexture", context.SceneViewDepth);
            VBAOMaterialInstance.SetTexture("BlueNoiseTexture", BlueNoiseTexture);

            VBAORenderTarget.SetColorAttachment(0, AOTexture);

            PushViewport(context.CommandList, new ViewportRect()
            {
                X = 0,
                Y = 0,
                Width = context.Viewport.Width / 2,
                Height = context.Viewport.Height / 2
            });

            context.CommandList.SetFramebuffer(VBAORenderTarget.GPU);

            context.CommandList.ClearColor(0, Colors.Black);

            API.RenderToScreen(context.CommandList, VBAOMaterialInstance);

            PopViewport(context.CommandList);

            context.SceneAO = AOTexture;

            PerCameraDescriptor.SetTexture(19, AOTexture.GPU);
        }
        public void RenderSceneBlurPass(ICommandList cmd, ref RenderContext context)
        {
            TextureDescription textureSceneBlurDescription = new()
            {
                Width = context.Viewport.Width / 2,
                Height = context.Viewport.Height / 2,
                Depth = 1,
                Format = TextureFormat.RGBA16_Float,
                Dimension = TextureDimension.Texture2D,
                ArraySize = 1,
                Samples = new TextureSampleDescription(1, 0),
                MipLevels = 1,
                Usage = TextureUsage.RenderTarget | TextureUsage.ShaderResource
            };

            Texture SceneBlurredHalfRes = context.Resources.GetOrCreateTexture("RENDERPASS_SCENE_BLURRED_HALF_RES", textureSceneBlurDescription);

            DownsampleMaterialInstance.SetTexture("INPUT_TEXTURE", context.SceneColor);

            DownsampleMaterialInstance.SetVector2("mipSize", new Vector2(context.Viewport.Width / 2, context.Viewport.Height / 2));

            BlurRenderTarget.SetColorAttachment(0, SceneBlurredHalfRes);
            PushViewport(cmd, new ViewportRect()
            {
                Width = context.Viewport.Width / 2,
                Height = context.Viewport.Height / 2
            });
            cmd.SetFramebuffer(BlurRenderTarget.GPU);
            API.RenderToScreen(cmd, DownsampleMaterialInstance);

            PopViewport(cmd);

            context.SceneColorBlurredHalfRes = SceneBlurredHalfRes;

        }
        public void UpdateCameraBuffer(CameraData cameraData)
        {
            CameraBuffer.Update(cameraData);
        }

        public void UpdatePerObjectData(Matrix4x4 model, float uniqueIdentifier = -1)
        {
            Matrix4x4.Invert(model, out var ModelMatrixInv);

            MeshRenderData meshRenderData = new()
            {
                ModelMatrix = model,
                ModelMatrixInv = ModelMatrixInv,
                UniqueIdentifier = uniqueIdentifier
            };


            PerObjectBuffer.Update(meshRenderData);
        }

        public void UpdateSceneData(RenderView view)
        {
            SceneBuffer.Update(new SceneData()
            {
                pointLightCount = (uint)view.PointLightCount,
                spotLightCount = (uint)view.SpotLightCount,
                directionalLightCount = (uint)view.DirectionalLightCount
            });
        }
        public void UpdatePerFrameData(PerFrameData data)
        {
            PerFrameBuffer.Update(data);
        }
        public void UpdateEnvironment(EnvironmentLighting env)
        {
            PerCameraDescriptor.SetTexture(15, env.Skybox.GPU);
            PerCameraDescriptor.SetTexture(16, env.Prefilter.GPU);
            PerCameraDescriptor.SetTexture(18, env.BrdfLut.GPU);

            PerCameraDescriptor.SetShaderStorageBuffer(17, env.SH9.GPU);
        }
        public void UpdateLights(RenderView view)
        {
            PointLightBuffer.Update(view.PointLights);
            SpotLightBuffer.Update(view.SpotLights);
            DirectionalLightBuffer.Update(view.DirectionalLights);

            PerCameraDescriptor.SetShaderStorageBuffer(10, PointLightBuffer.GPU);
            PerCameraDescriptor.SetShaderStorageBuffer(11, SpotLightBuffer.GPU);
            PerCameraDescriptor.SetShaderStorageBuffer(12, DirectionalLightBuffer.GPU);
        }
        public MaterialInstance GetDefaultMaterial()
        {
            return new MaterialInstance(DefaultMaterial);
        }

        // Method to draw a single mesh, i made it for the sky dome. expensive.
        public void Execute(ICommandList cmd, RenderMeshData item)
        {
            UpdatePerObjectData(item.render_transform, item.render_mesh.UniqueIdentifier);

            cmd.SetDescriptorSet(0, PerCameraDescriptor);
            cmd.SetDescriptorSet(2, PerObjectDescriptor);

            foreach (MeshSurface surface in item.render_mesh.Surfaces)
            {
                MaterialInstance material = surface.Material ?? item.render_material ?? NullMaterialInstance;

                ShaderPass pass = material.BaseMaterial.DefaultPass;

                cmd.SetPipeline(pass.GetPipeline(material.BaseMaterial.Variant, surface.VertexInfo));

                cmd.SetDescriptorSet(1, material.DescriptorSet);

                surface.Draw(cmd);
            }
        }

        public void Execute(ICommandList cmd, List<RenderMeshData> objects, MaterialInstance? overrideMaterial = null, PrimitiveType primitiveType = PrimitiveType.Triangles)
        {
            ShaderPass? currentPass = null;
            MaterialInstance? currentMaterial = null;
            VertexInfo? currentVertexInfo = null;

            cmd.SetDescriptorSet(2, PerObjectDescriptor);

            foreach (RenderMeshData renderMeshData in objects)
            {
                foreach (MeshSurface surface in renderMeshData.render_mesh.Surfaces)
                {
                    MaterialInstance material =
                        overrideMaterial ??
                        surface.Material ??
                        renderMeshData.render_material ??
                        NullMaterialInstance;

                    ShaderPass pass = material.BaseMaterial.DefaultPass;

                    if (pass != currentPass || surface.VertexInfo != currentVertexInfo)
                    {
                        currentPass = pass;
                        currentVertexInfo = surface.VertexInfo;

                        cmd.SetPipeline(
                            pass.GetPipeline(
                                material.BaseMaterial.Variant,
                                surface.VertexInfo,
                                primitiveType));

                        cmd.SetDescriptorSet(0, PerCameraDescriptor);
                    }

                    if (material != currentMaterial)
                    {
                        currentMaterial = material;
                        cmd.SetDescriptorSet(1, material.DescriptorSet);
                    }

                    UpdatePerObjectData(
                        renderMeshData.render_transform,
                        renderMeshData.render_mesh.UniqueIdentifier);

                    surface.Draw(cmd);
                }
            }
        }

        public void EnqueueGPUCommand(Action<ICommandList> action)
        {
            lock (pendingGpuCommands)
            {
                pendingGpuCommands.Enqueue(action);
            }
        }
        private void ExecutePendingGPUCommands(ICommandList cmd)
        {
            lock (pendingGpuCommands)
            {
                foreach (var action in pendingGpuCommands)
                {
                    action(cmd);
                }

                pendingGpuCommands.Clear();
            }
        }

        public void RegisterViewport(Viewport viewport)
        {
            if (RenderResources.ContainsKey(viewport))
            {
                throw new InvalidOperationException("Cannot register viewport again.");
            }
            RenderResources.Add(viewport, new RenderResourceCache(Engine.GraphicsDevice));
        }

        public void ResizeViewport(Viewport viewport)
        {
            if (!RenderResources.TryGetValue(viewport, out var cache))
                throw new InvalidOperationException(
                    "Viewport was not registered.");
            cache.Clear();
        }

        public void RemoveViewport(Viewport viewport)
        {
            if (RenderResources.TryGetValue(viewport, out RenderResourceCache? cache))
            {
                cache.Dispose();
                RenderResources.Remove(viewport);
                return;
            }

            throw new InvalidOperationException(
                "Viewport was not registered, cannot be removed.");
        }
        public RenderResourceCache? GetViewportResources(Viewport viewport)
        {
            if (RenderResources.TryGetValue(viewport, out var cache))
                return cache;

            return null;
        }
        public void Dispose()
        {
            ActiveTechnique?.Dispose();
            SkyRenderer.Dispose();
            GizmoRenderer.Dispose();

            DefaultMaterial.Dispose();
            NullMaterial.Dispose();
        }


    }
}
