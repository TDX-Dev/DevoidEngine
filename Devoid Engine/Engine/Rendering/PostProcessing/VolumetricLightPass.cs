using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.DebugTools;
using DevoidEngine.Engine.Utilities;
using DevoidGPU;
using System.Numerics;
using System.Runtime.InteropServices;

namespace DevoidEngine.Engine.Rendering.PostProcessing
{
    public class VolumetricLightPass : RenderGraphPass
    {
        [StructLayout(LayoutKind.Sequential)]
        struct VolumetricLightIndex
        {
            int currentLightIndex;
        }

        Texture2D output;
        Framebuffer framebuffer;

        UniformBuffer volumetricLightIndex;

        Shader volumetricShader;
        MaterialInstance material;
        Mesh ConeMesh;
        public override Texture2D OutputTexture => output;


        public VolumetricLightPass(int width, int height)
        {
            volumetricShader = new Shader("Engine/Content/Shaders/Volumetric/volumetric");
            material = new MaterialInstance(new Material(volumetricShader));

            volumetricLightIndex = new UniformBuffer(Marshal.SizeOf<VolumetricLightIndex>());

            Primitives.GenerateCone(32, out Vertex[] vertices, out int[] indices);
            ConeMesh = new Mesh();
            ConeMesh.SetVertices(vertices);
            ConeMesh.SetIndices(indices);


            output = new Texture2D(new TextureDescription()
            {
                Width = width,
                Height = height,
                Format = TextureFormat.RGBA16_Float,
                IsRenderTarget = true
            });

            framebuffer = new Framebuffer();
            framebuffer.AttachRenderTexture(output);
        }



        public override void Setup()
        {
            Write("VolumetricOutput");
        }

        public override void Execute(RenderGraphContext ctx)
        {
            Renderer.GraphicsDevice.SetBlendState(BlendMode.Additive);
            Renderer.GraphicsDevice.SetRasterizerState(CullMode.Front);

            framebuffer.Bind();
            framebuffer.Clear(new Vector4(0));
            volumetricLightIndex.Bind(3, ShaderStage.Fragment);

            Renderer.GetInputLayout(ConeMesh, volumetricShader);
            for (int i = 0; i < ctx.FrameContext.spotLights.Count; i++)
            {
                GPUSpotLight spotLight = ctx.FrameContext.spotLights[i];
                Matrix4x4 spotModel = GetSpotlightModel(spotLight);

                Renderer.SetupCamera(ctx.FrameContext.cameraData);
                Renderer.UpdatePerObjectData(spotModel);
                volumetricLightIndex.SetData(i);

                volumetricShader.Use();
                ConeMesh.Bind();
                ConeMesh.Draw();
            }



            //RenderAPI.RenderToBuffer(input, framebuffer);
            Renderer.GraphicsDevice.SetBlendState(BlendMode.Opaque);
            ctx.SetTexture("VolumetricOutput", output);
        }

        Matrix4x4 GetSpotlightModel(GPUSpotLight light)
        {
            float range = light.direction.W;

            float angle = light.outerCutoff; // already radians

            float radius = range * MathF.Tan(angle);

            Matrix4x4 scale = Matrix4x4.CreateScale(radius, radius, range);

            Vector3 dir = Vector3.Normalize(light.direction.AsVector3());

            Vector3 up = Vector3.UnitY;

            if (MathF.Abs(Vector3.Dot(dir, up)) > 0.999f)
                up = Vector3.UnitX;

            Matrix4x4 rotation = Matrix4x4.CreateWorld(Vector3.Zero, dir, up);

            Matrix4x4 translation =
                Matrix4x4.CreateTranslation(light.position.AsVector3());

            return scale * rotation * translation;
        }

        public override void Resize(int width, int height)
        {
            framebuffer.Resize(width, height);
        }
    }
}