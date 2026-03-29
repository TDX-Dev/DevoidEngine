using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.UI;
using DevoidEngine.Engine.Utilities;
using DevoidGPU;
using SharpFont;
using System.Numerics;

namespace DevoidEngine.Engine.Rendering
{
    public struct DebugCube
    {
        public Matrix4x4 Model;
        public Vector4 Color;
    }

    public struct DebugRect
    {
        public Matrix4x4 Model;
        public Vector4 Color;
    }

    public struct DebugMesh
    {
        public Matrix4x4 Model;
        public Mesh mesh;
        public Vector4 Color;
    }

    public static class DebugRenderSystem
    {
        static Shader debugShader;
        static MaterialInstance debugMaterial;

        static RenderState debug3DRenderState;
        static RenderState debug2DRenderState;

        static Mesh debugCube;
        static Mesh debugQuad;

        static List<DebugMesh> meshes = new();
        static List<DebugCube> cubes = new();
        static List<DebugRect> rects = new();

        public static bool AllowDebugDraw = true;

        static DebugRenderSystem()
        {
            debugCube = new Mesh();

            //debugCube.SetVertices(Primitives.GetCubeVertex());
            debugCube.SetVertices(Primitives.GetCubeLineVertices());
            debugCube.SetIndices(Primitives.GetCubeLineIndices());

            debugQuad = new Mesh();
            debugQuad.SetVertices(Primitives.GetQuadLineVertices());
            debugQuad.SetIndices(Primitives.GetQuadLineIndices());


            debugShader = new Shader("Engine/Content/Shaders/Testing/debugShader");
            debugMaterial = new MaterialInstance(new Material(debugShader));
            debug3DRenderState = new RenderState()
            {
                CullMode = CullMode.None,
                DepthTest = DepthTest.LessEqual,
                DepthWrite = true,
                FillMode = FillMode.Solid,
                PrimitiveType = PrimitiveType.Lines,
                BlendMode = BlendMode.Opaque
            };

            debug2DRenderState = new RenderState()
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

        public static void DrawCube(Vector3 min, Vector3 max, Matrix4x4 model)
        {
            Vector3 size = max - min;
            Vector3 center = (min + max) * 0.5f;

            Matrix4x4 localBox =
                Matrix4x4.CreateScale(size) *
                Matrix4x4.CreateTranslation(center);

            Matrix4x4 world = localBox * model;

            cubes.Add(new DebugCube
            {
                Model = world,
            });
        }

        public static void DrawMesh(Mesh mesh, Matrix4x4 model)
        {
            meshes.Add(new DebugMesh
            {
                mesh = mesh,
                Model = model
            });
        }

        public static void DrawRectUI(Matrix4x4 model)
        {
            rects.Add(new DebugRect()
            {
                Model = model,
                Color = new Vector4(0, 0.3f, 0.2f, 1)
            });
        }
        public static void Render(CameraData cameraData, Framebuffer cameraRenderSurface)
        {
            if (!AllowDebugDraw)
            {
                ClearDebugShapes();
                return;
            }

            cameraRenderSurface.Bind();

            Renderer.SetupCamera(cameraData); // 🔥 THIS IS MISSING

            List<RenderItem> renderItems3D = new List<RenderItem>();

            for (int i = 0; i < cubes.Count; i++)
            {
                renderItems3D.Add(new RenderItem()
                {
                    Material = debugMaterial,
                    Mesh = debugCube,
                    Model = cubes[i].Model
                });
            }

            for (int i = 0; i < meshes.Count; i++)
            {
                renderItems3D.Add(new RenderItem()
                {
                    Material = debugMaterial,
                    Mesh = meshes[i].mesh,
                    Model = meshes[i].Model
                });
            }

            Renderer.ExecuteDrawList(renderItems3D, debug3DRenderState);

            List<RenderItem> renderItems2D = new List<RenderItem>();

            Renderer.SetupCamera(UISystem.ScreenData);

            for (int i = 0; i < rects.Count; i++)
            {
                renderItems2D.Add(new RenderItem()
                {
                    Material = debugMaterial,
                    Mesh = debugQuad,
                    Model = rects[i].Model
                });
            }

            Renderer.ExecuteDrawList(renderItems2D, debug2DRenderState);


            renderItems3D.Clear();
            renderItems2D.Clear();
            ClearDebugShapes();
        }

        public static void ClearDebugShapes()
        {
            meshes.Clear();
            cubes.Clear();
            rects.Clear();
        }
    }
}