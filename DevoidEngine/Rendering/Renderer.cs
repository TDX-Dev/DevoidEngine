using DevoidEngine.Core;
using DevoidEngine.Util;
using DevoidGPU;
using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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

    public struct RendererConfig {
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
        public UniformBuffer CameraBuffer { get; private set; } = null!;
        public UniformBuffer SceneBuffer { get; private set;} = null!;
        public UniformBuffer PerObjectBuffer { get; private set; } = null!;
        public SkyRenderer SkyRenderer { get; private set; } = null!;

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

            cmd.SetViewport(0, 0, (int)viewport.Width, (int)viewport.Height);

            Camera camera = viewport.Camera3D.GetCamera();

            RenderContext context = new()
            {
                Viewport = viewport,
                Camera = camera,
                CommandList = cmd,
                Renderer = this,
                Resources = RenderResources[viewport]
            };

            renderView.Clear();
            World.BuildView(camera, ref renderView);

            camera.UpdateProjectionMatrix(((float)viewport.Width) / viewport.Height);
            UpdateCameraBuffer(camera.GetCameraData(new Vector2(viewport.Width, viewport.Height)));
            UpdateSceneData(renderView);
            UpdateLights(renderView);

            


            RenderTarget activeTechniqueTarget = ActiveTechnique.Render(context, renderView);

            ViewportBlitTarget.SetColorAttachment(0, viewport.OutputTexture!);
            cmd.SetFramebuffer(ViewportBlitTarget.GPU);
            API.RenderToScreen(cmd, activeTechniqueTarget.ColorTextures[0]!);
            
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
            ShaderPass pass = material.BaseMaterial.Shader.GetPass("Forward");
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

        public void Execute(ICommandList cmd, RenderView ctx)
        {
            ShaderPass? currentPass = null;
            MaterialInstance? currentMaterial = null;
            //Mesh? currentMesh = null;

            foreach (var item in ctx.Objects)
            {

                MaterialInstance material = item.render_material ?? NullMaterialInstance;

                ShaderPass pass = material.BaseMaterial.Shader.GetPass("Forward");

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
