using Assimp;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;
using DevoidGPU;
using System.Numerics;
using System.Runtime.InteropServices;

namespace DevoidEngine.Engine.Rendering.Shadows
{
    public class ShadowSystem
    {
        struct ShadowViewData
        {
            public Matrix4x4 Model;
            public Matrix4x4 LightVP;

            public Vector3 LightPosition;
            public float LightRange;
        }

        static ShadowAtlas atlas = null!;

        static List<ShadowData> shadowData = new();

        static StorageBuffer<ShadowData> shadowBuffer = null!;
        static UniformBuffer shadowViewInfoBuffer = null!;

        ShadowViewData viewData;

        static Shader shadowShader = null!;

        const int MAX_SHADOWS = 64;

        public void Initialize()
        {
            atlas = new ShadowAtlas(2048, 1024);

            shadowBuffer = new StorageBuffer<ShadowData>(MAX_SHADOWS);

            shadowViewInfoBuffer = new UniformBuffer(Marshal.SizeOf<ShadowViewData>());
            viewData = new ShadowViewData();
            //shadowBuffer = new UniformBuffer(1024 * sizeof(float) * 20);

            shadowShader = ShaderLibrary.GetShader("Shadow/DEPTH_ONLY")!;
        }

        public void RenderShadowMaps(CameraRenderContext ctx)
        {
            atlas.Reset();
            shadowData.Clear();

            atlas.Framebuffer.Bind();
            atlas.Framebuffer.Clear(new Vector4(1), true);

            int shadowIndex = 0;

            for (int i = 0; i < ctx.spotLights.Count; i++)
            {
                var light = ctx.spotLights[i];

                // Light does not want shadows
                if (light.shadowIndex == -1)
                    continue;

                if (shadowIndex >= MAX_SHADOWS)
                    break;

                int tile = atlas.AllocateTile(
                    out float offsetX,
                    out float offsetY,
                    out float scale);

                atlas.SetViewport(tile);
                Renderer.GraphicsDevice.SetDepthState(DepthTest.Disabled, false);
                RenderAPI.ClearView(new Vector4(light.direction.W));
                Renderer.GraphicsDevice.SetDepthState(DepthTest.Less, true);
                //atlas.Framebuffer.Clear(new Vector4(light.direction.W), false);

                Vector3 position = new Vector3(light.position.X, light.position.Y, light.position.Z);

                Matrix4x4 lightViewProj = ComputeLightMatrix(light);

                RenderDepth(lightViewProj, position, light.direction.W, ctx);

                shadowData.Add(new ShadowData
                {
                    LightViewProj = Matrix4x4.Transpose(lightViewProj),
                    AtlasOffset = new Vector2(offsetX, offsetY),
                    AtlasScale = new Vector2(scale, scale),
                    LightPosition = position
                });

                // assign shadow index to light
                light.shadowIndex = shadowIndex;

                ctx.spotLights[i] = light;

                shadowIndex++;
            }

            if (shadowData.Count > 0)
                shadowBuffer.SetData(shadowData, shadowData.Count);

            shadowBuffer.Bind(13, ShaderStage.Vertex | ShaderStage.Fragment);
            Renderer.GraphicsDevice.UnbindFramebuffer();
            atlas.DistanceTexture.Bind(9);
        }

        Matrix4x4 ComputeLightMatrix(GPUSpotLight light)
        {
            Vector3 pos = new(light.position.X, light.position.Y, light.position.Z);

            float radius = light.direction.W;

            Vector3 dir = new(
                light.direction.X,
                light.direction.Y,
                light.direction.Z);

            dir = Vector3.Normalize(dir);

            Vector3 up = Vector3.UnitY;

            if (MathF.Abs(Vector3.Dot(dir, up)) > 0.999f)
            {
                up = Vector3.UnitZ;
            }

            Matrix4x4 view = Matrix4x4.CreateLookAt(
                pos,
                pos + dir,
                up);

            float far = radius;

            Matrix4x4 proj = Matrix4x4.CreatePerspectiveFieldOfView(
                light.outerCutoff * 2,
                1.0f,
                0.1f,
                far);


            return view * proj;
        }

        void RenderDepth(Matrix4x4 lightVP, Vector3 lightPosition, float lightRange, CameraRenderContext ctx)
        {
            shadowShader.Use();

            shadowViewInfoBuffer.Bind(1, ShaderStage.Vertex | ShaderStage.Fragment);
            Renderer.GraphicsDevice.SetRasterizerState(CullMode.Front);

            foreach (var item in ctx.renderItems3D)
            {
                Matrix4x4 model = item.Model;


                viewData = new ShadowViewData
                {
                    Model = model,
                    LightVP = lightVP,
                    LightPosition = lightPosition,
                    LightRange = lightRange
                };

                shadowViewInfoBuffer.SetData(viewData);

                Renderer.GetInputLayout(item.Mesh, shadowShader).Bind();

                item.Mesh.Bind();
                item.Mesh.Draw();
            }
        }
    }
}