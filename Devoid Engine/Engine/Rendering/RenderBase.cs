using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.UI;
using DevoidGPU;
using System.Numerics;
using System.Runtime.InteropServices;

namespace DevoidEngine.Engine.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    struct MeshRenderData
    {
        public Matrix4x4 ModelMatrix;
        public Matrix4x4 ModelMatrixInv;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CameraData
    {
        public Matrix4x4 View;
        public Matrix4x4 Projection;
        public Matrix4x4 InverseProjection;
        public Vector3 Position;
        public float NearClip;
        public float FarClip;
        public Vector2 ScreenSize;
        private float _padding0;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SceneData
    {
        public uint pointLightCount;
        public uint spotLightCount;
        public uint directionalLightCount;
        public int _padding;
    }

    public static class RenderBase
    {
        public static Texture2D Output { get; set; }

        public static IRenderTechnique ActiveRenderTechnique;

        // Renderer objects
        static MeshRenderData _meshRenderData;
        static UniformBuffer _meshRenderDataBuffer;
        static UniformBuffer _cameraDataBuffer;

        public static void SetupCamera(CameraData cameraData)
        {
            // Per camera data goes here
            _cameraDataBuffer.SetData(cameraData);
            _cameraDataBuffer.Bind(RenderBindConstants.CameraDataBindSlot, ShaderStage.Vertex | ShaderStage.Fragment);
        }


        public unsafe static void Initialize(int width, int height)
        {
            _meshRenderData = new MeshRenderData();
            _meshRenderDataBuffer = new UniformBuffer(sizeof(MeshRenderData));

            _cameraDataBuffer = new UniformBuffer(sizeof(CameraData));

            // TESTING
            ActiveRenderTechnique = new ForwardRenderTechnique();

            ActiveRenderTechnique?.Initialize(width, height);
        }

        public static void Resize(int width, int height)
        {
            ActiveRenderTechnique?.Resize(width, height);
        }

        public static void Render(CameraRenderContext ctx)
        {
            if (ActiveRenderTechnique == null)
                Console.WriteLine("[Renderer]: Render technique was not set. No Object rendered.");
            Output = ActiveRenderTechnique?.Render(ctx);
            Renderer.graphicsDevice.UnbindAllShaderResources();
            RenderAPI.RenderToBuffer(Output, ctx.cameraTargetSurface);
        }

        public static IInputLayout GetInputLayout(Mesh mesh, Shader shader)
        {
            var key = (mesh.VertexBuffer.GetVertexInfo(), shader.vShader);
            if (!InputLayoutManager.inputLayoutCache.TryGetValue(key, out var layout))
            {
                layout = Renderer.graphicsDevice.CreateInputLayout(mesh.VertexBuffer.GetVertexInfo(), shader.vShader);
                InputLayoutManager.inputLayoutCache[key] = layout;
            }
            return layout;
        }

        public static void GetFinalOutput()
        {

        }

        public static void Execute(List<RenderItem> items, RenderState renderState)
        {
            if (items.Count == 0) { return; }

            MaterialInstance currentMaterial = null;
            Shader currentShader = null;
            Mesh currentMesh = null;

            ApplyRenderState(renderState);

            int currentObjectDataBindSlot = -1;

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];


                if (item.Material != currentMaterial)
                {
                    currentMaterial = item.Material;
                    if (item.Material.BaseMaterial.Shader != currentShader)
                    {
                        currentShader = item.Material.BaseMaterial.Shader;

                        int perObjectBindSlot = currentShader.vShader.ReflectionData.GetUniformBufferSlot("perObject");

                        if (perObjectBindSlot != currentObjectDataBindSlot)
                        {
                            currentObjectDataBindSlot = perObjectBindSlot;

                            if (currentObjectDataBindSlot == -1)
                            {
                                Console.WriteLine("Shader does not implement PerObject uniform buffer.\nObject data cannot be sent to shader.");
                                continue;
                            }

                            _meshRenderDataBuffer.Bind(currentObjectDataBindSlot, ShaderStage.Vertex);
                        }

                        currentShader.Use();
                    }
                    currentMaterial.Bind();
                }

                if (item.Mesh != currentMesh) { currentMesh = item.Mesh; }
                if (currentMesh == null) continue;

                UpdatePerObjectData(item.Model);

                currentMesh.Bind();
                RenderBase.GetInputLayout(currentMesh, currentShader).Bind();

                currentMesh.Draw();
            }
        }

        static void UpdatePerObjectData(Matrix4x4 model)
        {
            _meshRenderData = new MeshRenderData()
            {
                ModelMatrix = model
            };
            Matrix4x4.Invert(model, out _meshRenderData.ModelMatrixInv);
            _meshRenderDataBuffer.SetData(_meshRenderData);
        }

        public static Matrix4x4 BuildModel(Vector3 pos, Vector3 scale, Quaternion rot)
        {
            return
                Matrix4x4.CreateFromQuaternion(rot) *
                Matrix4x4.CreateScale(scale) *
                Matrix4x4.CreateTranslation(pos);
        }

        static void ApplyRenderState(RenderState renderState)
        {
            Renderer.graphicsDevice.SetBlendState(renderState.BlendMode);
            Renderer.graphicsDevice.SetDepthState(renderState.DepthTest, renderState.DepthWrite);
            Renderer.graphicsDevice.SetRasterizerState(renderState.CullMode, renderState.FillMode);
            Renderer.graphicsDevice.SetPrimitiveType(renderState.PrimitiveType);
        }
    }
}
