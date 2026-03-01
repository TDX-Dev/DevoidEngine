using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;
using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Rendering
{
    public struct DebugCube
    {
        public Matrix4x4 Model;
        public Vector4 Color;
    }

    public static class DebugRenderSystem
    {
        static Shader debugShader;
        static MaterialInstance debugMaterial;
        static RenderState debugRenderState;

        static Mesh debugCube;

        static List<DebugCube> cubes = new();

        public static bool AllowDebugDraw = true;

        static DebugRenderSystem()
        {
            debugCube = new Mesh();

            //debugCube.SetVertices(Primitives.GetCubeVertex());
            debugCube.SetVertices(Primitives.GetCubeLineVertices());
            debugCube.SetIndices(Primitives.GetCubeLineIndices());


            debugShader = new Shader("Engine/Content/Shaders/Testing/debugShader");
            debugMaterial = new MaterialInstance(new Material(debugShader));
            debugRenderState = new RenderState()
            {
                CullMode = CullMode.None,
                DepthTest = DepthTest.Disabled,
                DepthWrite = false,
                FillMode = FillMode.Solid,
                PrimitiveType = PrimitiveType.Lines,
                BlendMode = BlendMode.Opaque
            };
        }

        public static void DrawCube(Matrix4x4 model)
        {
            cubes.Add(new DebugCube
            {
                Model = model
            });
        }

        public static void Render(CameraData cameraData, Framebuffer cameraRenderSurface)
        {
            if (cubes.Count == 0 || !AllowDebugDraw)
            {
                ClearDebugShapes();
                return;
            }

            cameraRenderSurface.Bind();

            RenderBase.SetupCamera(cameraData); // 🔥 THIS IS MISSING

            List<RenderItem> renderItems = new List<RenderItem>();

            for (int i = 0; i < cubes.Count; i++)
            {
                renderItems.Add(new RenderItem()
                {
                    Material = debugMaterial,
                    Mesh = debugCube,
                    Model = cubes[i].Model
                });
            }

            RenderBase.Execute(renderItems, debugRenderState);

            ClearDebugShapes();
        }

        public static void ClearDebugShapes()
        {
            cubes.Clear();
        }
    }
}
