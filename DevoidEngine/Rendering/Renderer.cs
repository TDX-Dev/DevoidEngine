using DevoidEngine.Core;
using DevoidEngine.UI;
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

        public RenderWorld World { get; private set; } = null!;
        public RenderAPI API { get; private set; } = null!;

        public Material DefaultMaterial { get; private set; } = null!;
        public Material NullMaterial { get; private set; } = null!;
        public MaterialInstance NullMaterialInstance { get; private set; } = null!;
        public Shader DefaultShader { get; private set; } = null!;

        public RenderTarget ViewportBlitTarget { get; private set; } = null!;
        public RenderTarget UIRenderTarget { get; private set; } = null!;
        public UniformBuffer CameraBuffer { get; private set; } = null!;
        public UniformBuffer SceneBuffer { get; private set; } = null!;
        public UniformBuffer PerObjectBuffer { get; private set; } = null!;
        public SkyRenderer SkyRenderer { get; private set; } = null!;

        public EnvironmentLighting Environment => SkyRenderer.Environment;

        private IDescriptorLayout PerCameraDescriptorLayout = null!;
        private IDescriptorSet PerCameraDescriptor = null!;

        private IDescriptorLayout PerObjectDescriptorLayout = null!;
        private IDescriptorSet PerObjectDescriptor = null!;

        private Stack<ViewportRect> viewportStack = null!;
        private ViewportRect currentViewport;

        private RenderView renderView;

        private ShaderStorageBuffer<GPUPointLight> PointLightBuffer = null!;
        private ShaderStorageBuffer<GPUSpotLight> SpotLightBuffer = null!;
        private ShaderStorageBuffer<GPUDirectionalLight> DirectionalLightBuffer = null!;

        private Pool<RenderMeshData> uiRenderPool = null!;
        private readonly List<RenderMeshData> uiRenderCache = [];

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

        public void PopViewport(ICommandList cmd)
        {
            if (viewportStack.Count == 0)
                return;
            currentViewport = viewportStack.Pop();

            cmd.SetViewport(
                currentViewport.X,
                currentViewport.Y,
                currentViewport.Width,
                currentViewport.Height);
        }

        //private readonly IDescriptorLayout PerObjectDescriptorLayout = null!;
        //private readonly IDescriptorSet PerObjectDescriptor = null!;

        private Dictionary<Viewport, RenderResourceCache> RenderResources = null!;


        public void Initialize(RendererConfig config)
        {
            if (Engine.GraphicsDevice == null)
                throw new Exception("Graphics device not initialized yet.");

            viewportStack = new Stack<ViewportRect>();
            uiRenderPool = new Pool<RenderMeshData>();
            RenderResources = [];

            ActiveTechnique = config.Technique switch
            {
                RenderTechnique.Forward => new ForwardRenderTechnique(),
                _ => throw new NotImplementedException(nameof(config.Technique) + " is not implemented."),
            };

            ShaderLibrary = new ShaderLibrary();
            World = new RenderWorld();

            DefaultShader = Shader.FromDescriptorFile(Engine.GraphicsDevice, "Content/DevoidShaderDescriptors/pbr_mat.dsd");
            DefaultMaterial = new Material(DefaultShader);

            Shader NullShader = Shader.FromDescriptorFile(Engine.GraphicsDevice, "Content/DevoidShaderDescriptors/null_mat.dsd");
            NullMaterial = new Material(NullShader);
            NullMaterialInstance = new MaterialInstance(NullMaterial);

            PerCameraDescriptorLayout = Engine.GraphicsDevice.CreateDescriptorLayout([
                new() {
                    Binding = 0,
                    Stages = DevoidGPU.ShaderStage.Vertex | DevoidGPU.ShaderStage.Fragment,
                    Type = DescriptorType.UniformBuffer
                },
                new() {
                    Binding = 2,
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
                }
            ]);

            PerCameraDescriptor = Engine.GraphicsDevice.CreateDescriptorSet(PerCameraDescriptorLayout);

            PerObjectDescriptorLayout = Engine.GraphicsDevice.CreateDescriptorLayout([
                new() {
                    Binding = 1,
                    Stages = DevoidGPU.ShaderStage.Vertex,
                    Type = DescriptorType.UniformBuffer
                }
            ]);

            PerObjectDescriptor = Engine.GraphicsDevice.CreateDescriptorSet(PerObjectDescriptorLayout);

            CameraBuffer = UniformBuffer.Create(ResourceUsage.Dynamic, (uint)Unsafe.SizeOf<CameraData>());
            PerObjectBuffer = UniformBuffer.Create(ResourceUsage.Dynamic, (uint)Unsafe.SizeOf<MeshRenderData>());
            SceneBuffer = UniformBuffer.Create(ResourceUsage.Dynamic, (uint)Unsafe.SizeOf<SceneData>());

            PerCameraDescriptor.SetUniformBuffer(2, SceneBuffer.GPU);
            PerObjectDescriptor.SetUniformBuffer(1, PerObjectBuffer.GPU);

            API = new RenderAPI();

            ViewportBlitTarget = RenderTarget.Create(1);
            UIRenderTarget = RenderTarget.Create(1);

            renderView = new RenderView();

            PointLightBuffer = ShaderStorageBuffer<GPUPointLight>.Create(ResourceUsage.Dynamic, Renderer.MAX_POINT_LIGHTS);
            SpotLightBuffer = ShaderStorageBuffer<GPUSpotLight>.Create(ResourceUsage.Dynamic, Renderer.MAX_SPOT_LIGHTS);
            DirectionalLightBuffer = ShaderStorageBuffer<GPUDirectionalLight>.Create(ResourceUsage.Dynamic, Renderer.MAX_DIRECTIONAL_LIGHTS);

            SkyRenderer = new SkyRenderer();

            ActiveTechnique.Initialize();
        }

        public void Render(ICommandList cmd, Viewport viewport)
        {
            if (viewport.Camera3D == null || ActiveTechnique == null)
                return;

            PopViewport(cmd);
            PushViewport(cmd, new ViewportRect()
            {
                Width = viewport.Width,
                Height = viewport.Height,
                X = 0,
                Y = 0,
            });

            Camera camera = viewport.Camera3D.GetCamera();

            RenderResourceCache viewportResources = RenderResources[viewport];

            RenderContext context = new()
            {
                Viewport = viewport,
                Camera = camera,
                CommandList = cmd,
                Renderer = this,
                Resources = viewportResources
            };

            renderView.Clear();
            World.BuildView(camera, ref renderView);

            camera.UpdateProjectionMatrix(((float)viewport.Width) / viewport.Height);
            UpdateCameraBuffer(camera.GetCameraData(new Vector2(viewport.Width, viewport.Height)));
            UpdateSceneData(renderView);
            UpdateLights(renderView);

            SkyRenderer.Render(context);
            RenderTarget activeTechniqueTarget = ActiveTechnique.Render(context, renderView);

            RenderUI(cmd, viewport, viewportResources);

            ViewportBlitTarget.SetColorAttachment(0, viewport.OutputTexture!);
            cmd.SetFramebuffer(ViewportBlitTarget.GPU);
            API.RenderToScreen(cmd, activeTechniqueTarget.ColorTextures[0]!);
            API.RenderToScreen(cmd, UIRenderTarget.ColorTextures[0]!);
        }

        public void RenderUI(ICommandList cmd, Viewport viewport, RenderResourceCache resources)
        {
            UIContext context = viewport.UIContext;
            context.DrawList.Clear();

            TextureDescription uiColorTexture = new()
            {
                Width = viewport.Width,
                Height = viewport.Height,
                Depth = 1,
                Format = TextureFormat.RGBA16_Float,
                Dimension = TextureDimension.Texture2D,
                ArraySize = 1,
                Samples = new TextureSampleDescription(1, 0),
                MipLevels = 1,
                Usage = TextureUsage.RenderTarget | TextureUsage.ShaderResource
            };

            Texture UIColorTexture = resources.GetOrCreateTexture("UI_RENDER_COLOR", uiColorTexture);

            Matrix4x4 ortho = Matrix4x4.CreateOrthographicOffCenter(
                0f, viewport.Width,
                viewport.Height, 0f,
                -1f, 1f
            );

            CameraData ScreenData = new()
            {
                View = Matrix4x4.Identity,
                Projection = ortho,
                CameraPosition = Vector3.Zero,
                NearClip = -1f,
                FarClip = 1f,
                ScreenSize = new Vector2(viewport.Width, viewport.Height)
            };

            Matrix4x4.Invert(ortho, out ScreenData.InverseProjection);

            UpdateCameraBuffer(ScreenData);

            UIRenderTarget.SetColorAttachment(0, UIColorTexture);

            cmd.SetFramebuffer(UIRenderTarget.GPU);

            cmd.ClearColor(0, Vector4.Zero);

            foreach (var canvas in context.Canvases)
            {
                canvas.Render(context.DrawList, 0);
            }

            List<UICommand> commands = context.DrawList.Commands;

            Matrix4x4 cachedTransform = Matrix4x4.Identity;


            for (var i = 0; i < commands.Count; i++)
            {
                var command = commands[i];

                switch (command.Type)
                {
                    case UICommandType.PushTransform:
                        {
                            cachedTransform = command.Transform.Transform;
                            break;
                        }
                    case UICommandType.PopTransform:
                        {
                            cachedTransform = Matrix4x4.Identity;
                            break;
                        }

                    case UICommandType.Quad:
                        {
                            RenderMeshData meshData = uiRenderPool.Get();

                            meshData.render_mesh = PrimitiveMeshes.GetQuad();
                            meshData.render_material = command.Quad.Material;

                            Matrix4x4 quadTransform =
                                Matrix4x4.CreateScale(command.Quad.Rect.Size.X, command.Quad.Rect.Size.Y, 1) *
                                Matrix4x4.CreateTranslation(command.Quad.PivotOffset.X, command.Quad.PivotOffset.Y, 0f) *
                                Matrix4x4.CreateRotationZ(command.Quad.Rotation) *
                                Matrix4x4.CreateTranslation(command.Quad.Rect.Position.X, command.Quad.Rect.Position.Y, UIResources.OrderEpsilon * command.Quad.Order) *
                                cachedTransform;

                            meshData.render_transform = quadTransform;

                            uiRenderCache.Add(meshData);

                            break;
                        }

                    case UICommandType.Text:
                        {
                            RenderMeshData meshData = uiRenderPool.Get();

                            meshData.render_mesh = command.Text.TextMesh;
                            meshData.render_material = command.Text.Material;

                            Matrix4x4 quadTransform =
                                //Matrix4x4.CreateScale(command.Text.Rect.Size.X, command.Text.Rect.Size.Y, 1) *
                                Matrix4x4.CreateTranslation(command.Text.PivotOffset.X, command.Text.PivotOffset.Y, 0f) *
                                Matrix4x4.CreateRotationZ(command.Text.Rotation) *
                                Matrix4x4.CreateTranslation(command.Text.Rect.Position.X, command.Text.Rect.Position.Y, UIResources.OrderEpsilon * command.Text.Order) *
                                cachedTransform;

                            meshData.render_transform = quadTransform;

                            uiRenderCache.Add(meshData);

                            break;
                        }
                }
            }

            Execute(cmd, uiRenderCache);

            for (var i = 0; i < uiRenderCache.Count; i++)
            {
                uiRenderPool.Return(uiRenderCache[i]);
            }


            uiRenderCache.Clear();
        }

        public void UpdateCameraBuffer(CameraData cameraData)
        {
            CameraBuffer.Update(cameraData);
            PerCameraDescriptor.SetUniformBuffer(0, CameraBuffer.GPU);
        }

        public void UpdatePerObjectData(Matrix4x4 model)
        {
            Matrix4x4.Invert(model, out var ModelMatrixInv);

            MeshRenderData meshRenderData = new()
            {
                ModelMatrix = model,
                ModelMatrixInv = ModelMatrixInv
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
            MaterialInstance material = item.render_material ?? NullMaterialInstance;
            ShaderPass pass = material.BaseMaterial.DefaultPass;
            cmd.SetPipeline(pass.Pipeline);
            cmd.SetDescriptorSet(
                        0,
                        PerCameraDescriptor);

            cmd.SetDescriptorSet(
                1,
                material.DescriptorSet);

            UpdatePerObjectData(item.render_transform);
            cmd.SetDescriptorSet(2, PerObjectDescriptor);

            item.render_mesh.Draw(cmd);
        }

        public void Execute(ICommandList cmd, List<RenderMeshData> objects)
        {
            ShaderPass? currentPass = null;
            MaterialInstance? currentMaterial = null;
            //Mesh? currentMesh = null;



            foreach (var item in objects)
            {

                MaterialInstance material = item.render_material ?? NullMaterialInstance;

                ShaderPass pass = material.BaseMaterial.DefaultPass;

                if (pass != currentPass)
                {
                    currentPass = pass;

                    cmd.SetPipeline(pass.Pipeline);

                    cmd.SetDescriptorSet(
                        0,
                        PerCameraDescriptor);
                }

                if (item.render_material != currentMaterial)
                {
                    currentMaterial = item.render_material;

                    cmd.SetDescriptorSet(
                        1,
                        material.DescriptorSet);
                }

                UpdatePerObjectData(item.render_transform);
                cmd.SetDescriptorSet(2, PerObjectDescriptor);


                item.render_mesh.Draw(cmd);
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


        public void Dispose()
        {
            ActiveTechnique?.Dispose();
        }


    }
}
