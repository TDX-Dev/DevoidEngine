using DevoidEngine.Components;
using DevoidEngine.Core;
using DevoidEngine.Gizmos;
using DevoidEngine.Util;
using DevoidGPU;
using System.Numerics;

namespace DevoidEngine.Rendering
{
    public sealed class GizmoRenderer : IDisposable
    {
        public RenderTarget GizmoRenderTarget { get; private set; } = null!;
        public Shader GizmoShader { get; private set; } = null!;
        public MaterialInstance GizmoMaterialInstance { get; private set; } = null!;

        private Pool<RenderMeshData> gizmoRenderDataPool = null!;
        private Pool<GizmoBatch> gizmoBatchPool = null!;
        private readonly List<GizmoBatch> gizmoBatches = [];

        public void Initialize(IGraphicsDevice device, string basePath)
        {
            gizmoRenderDataPool = new Pool<RenderMeshData>();
            gizmoBatchPool = new Pool<GizmoBatch>();
            GizmoRenderTarget = RenderTarget.Create(1);

            GizmoShader = Shader.FromDescriptorFile(
                device,
                Path.Combine(basePath, "Content/DevoidShaderDescriptors/gizmo.dsd")
            );
            GizmoMaterialInstance = new MaterialInstance(new Material(GizmoShader));
        }

        public void Render(RenderContext context, Viewport viewport, RenderResourceCache resources)
        {
            int gizmoDrawCalls = 0;
            GizmoContext gizmoContext = viewport.GizmoContext;
            Engine.GizmoSystem.Draw(gizmoContext);


            TextureDescription gizmoColorDesc = new()
            {
                Width = viewport.Width,
                Height = viewport.Height,
                Depth = 1,
                Format = TextureFormat.RGBA8_UNorm,
                Dimension = TextureDimension.Texture2D,
                ArraySize = 1,
                Samples = new TextureSampleDescription(1, 0),
                MipLevels = 1,
                Usage = TextureUsage.RenderTarget | TextureUsage.ShaderResource
            };

            TextureDescription gizmoDepthDesc = new()
            {
                Width = viewport.Width,
                Height = viewport.Height,
                Depth = 1,
                Format = TextureFormat.Depth24_Stencil8,
                Dimension = TextureDimension.Texture2D,
                ArraySize = 1,
                Samples = new TextureSampleDescription(1, 0),
                MipLevels = 1,
                Usage = TextureUsage.DepthStencil
            };

            GizmoMaterialInstance.SetTexture("DepthTexture", context.SceneDepth);

            Texture gizmoColorTexture = resources.GetOrCreateTexture("GIZMO_RENDER_COLOR", gizmoColorDesc);
            Texture gizmoDepthTexture = resources.GetOrCreateTexture("GIZMO_RENDER_DEPTH", gizmoDepthDesc);
            GizmoRenderTarget.SetColorAttachment(0, gizmoColorTexture);
            GizmoRenderTarget.SetDepthAttachment(gizmoDepthTexture);

            context.CommandList.SetFramebuffer(GizmoRenderTarget.GPU);
            context.CommandList.ClearColor(0, Vector4.Zero);
            context.CommandList.ClearDepthStencil(1, 0);

            if (gizmoContext.DrawList.Commands.Count == 0) return;

            Camera renderCamera = context.Camera;

            GizmoBatch? currentBatch = null;

            foreach (GizmoDrawCommand command in gizmoContext.DrawList.Commands)
            {
                RenderMeshData meshData = gizmoRenderDataPool.Get();

                meshData.render_material = GizmoMaterialInstance;

                switch (command.Type)
                {
                    case GizmoDrawCommandType.WireBox:
                        {
                            meshData.render_mesh = PrimitiveMeshes.GetWireCube();

                            Vector3 size = command.Max - command.Min;
                            Vector3 center = (command.Min + command.Max) * 0.5f;

                            meshData.render_transform =
                                Matrix4x4.CreateScale(size) *
                                Matrix4x4.CreateTranslation(center);

                            break;
                        }

                    case GizmoDrawCommandType.Box:
                        {
                            meshData.render_mesh = PrimitiveMeshes.GetCube();

                            Vector3 size = command.Max - command.Min;
                            Vector3 center = (command.Min + command.Max) * 0.5f;

                            meshData.render_transform =
                                Matrix4x4.CreateScale(size) *
                                Matrix4x4.CreateTranslation(center);

                            break;
                        }

                    case GizmoDrawCommandType.Circle:
                        {
                            meshData.render_mesh =
                                PrimitiveMeshes.GetCenteredCircle();

                            Matrix4x4 viewMatrix = renderCamera.View;

                            Matrix4x4 rotationMatrix = new(
                                viewMatrix.M11, viewMatrix.M21, viewMatrix.M31, 0,
                                viewMatrix.M12, viewMatrix.M22, viewMatrix.M32, 0,
                                viewMatrix.M13, viewMatrix.M23, viewMatrix.M33, 0,
                                0, 0, 0, 1
                            );

                            meshData.render_transform =
                                Matrix4x4.CreateScale(command.Radius) *
                                rotationMatrix *
                                Matrix4x4.CreateTranslation(command.Center);

                            break;
                        }
                }

                if (meshData.render_mesh == null)
                {
                    gizmoRenderDataPool.Return(meshData);
                    continue;
                }

                GizmoBatchState state = new(
                    command.Material,
                    GetPrimitiveType(command));

                if (currentBatch == null || currentBatch.State != state)
                {
                    currentBatch = gizmoBatchPool.Get();
                    currentBatch.Reset();
                    currentBatch.State = state;

                    gizmoBatches.Add(currentBatch);
                }

                currentBatch.RenderData.Add(meshData);
            }

            foreach (GizmoBatch batch in gizmoBatches)
            {
                GizmoMaterial material = batch.State.Material;

                GizmoMaterialInstance.SetVector4(
                    "Color",
                    material.Color);

                if (material.Texture != null)
                {
                    GizmoMaterialInstance.SetTexture(
                        "Texture",
                        material.Texture);
                }

                GizmoMaterialInstance.SetVector2(
                    "UVMin",
                    material.UVMin);

                GizmoMaterialInstance.SetVector2(
                    "UVMax",
                    material.UVMax);

                context.Renderer.Execute(
                    context.CommandList,
                    batch.RenderData,
                    null,
                    batch.State.PrimitiveType);
                gizmoDrawCalls += batch.RenderData.Count;
            }

            ClearBatches();
            //Console.WriteLine($"Gizmo draw calls: {gizmoDrawCalls}");
            viewport.GizmoContext.DrawList.Clear();
        }

        private static PrimitiveType GetPrimitiveType(GizmoDrawCommand command)
        {
            return command.Type == GizmoDrawCommandType.WireBox
                ? PrimitiveType.Lines
                : PrimitiveType.Triangles;
        }

        private void ClearBatches()
        {
            foreach (GizmoBatch batch in gizmoBatches)
            {
                foreach (RenderMeshData renderData in batch.RenderData)
                {
                    gizmoRenderDataPool.Return(renderData);
                }

                batch.Reset();
                gizmoBatchPool.Return(batch);
            }

            gizmoBatches.Clear();
        }

        public void Dispose()
        {
            //GizmoRenderTarget?.Dispose();
            GizmoShader?.Dispose();
            GizmoMaterialInstance?.Dispose();
            gizmoBatches.Clear();
        }
    }
}