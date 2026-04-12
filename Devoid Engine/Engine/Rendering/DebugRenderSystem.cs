using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.UI;
using DevoidEngine.Engine.Utilities;
using DevoidGPU;
using SharpFont;
using System.Numerics;
using System.Runtime.InteropServices;

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
        public MaterialInstance materialOverride;
        public Vector4 Color;
    }

    public struct DebugMesh
    {
        public Matrix4x4 Model;
        public Mesh mesh;
        public Vector4 Color;
    }

    public struct DebugShaderData
    {
        public Vector4 Color;
    }

    public static class DebugRenderSystem
    {
        static Shader debugShader;
        static MaterialInstance debugMaterial;

        static RenderState debug3DRenderState;
        static RenderState debug2DRenderState;
        static RenderState debug2DWireRenderState;

        static Mesh debugCube;
        static Mesh debugQuad;
        static Mesh debugQuadFilled;

        static List<DebugMesh> meshes = new();
        static List<DebugCube> cubes = new();
        static List<DebugRect> rects = new();

        static UniformBuffer debugShaderDataBuffer;
        static DebugShaderData debugShaderData;
        static DebugShaderData lastDebugShaderData;

        public static bool AllowDebugDraw = true;

        static DebugRenderSystem()
        {
            debugShaderData = new DebugShaderData();
            debugShaderDataBuffer = new UniformBuffer(Marshal.SizeOf<DebugShaderData>());

            debugCube = new Mesh();

            //debugCube.SetVertices(Primitives.GetCubeVertex());
            debugCube.SetVertices(Primitives.GetCubeLineVertices());
            debugCube.SetIndices(Primitives.GetCubeLineIndices());

            debugQuad = new Mesh();
            debugQuad.SetVertices(Primitives.GetQuadLineVertices());
            debugQuad.SetIndices(Primitives.GetQuadLineIndices());

            debugQuadFilled = new Mesh();
            debugQuadFilled.SetVertices(Primitives.GetQuadVertex());


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

            debug2DWireRenderState = new RenderState()
            {
                CullMode = CullMode.None,
                DepthTest = DepthTest.Disabled,
                DepthWrite = false,
                FillMode = FillMode.Wireframe,
                PrimitiveType = PrimitiveType.Lines,
                BlendMode = BlendMode.Opaque
            };

            debug2DRenderState = new RenderState()
            {
                CullMode = CullMode.None,
                DepthTest = DepthTest.Disabled,
                DepthWrite = false,
                FillMode = FillMode.Solid,
                PrimitiveType = PrimitiveType.Triangles,
                BlendMode = BlendMode.AlphaBlend
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

        public static void DrawMesh(Mesh mesh, Matrix4x4 model, Vector4 Color)
        {
            meshes.Add(new DebugMesh
            {
                mesh = mesh,
                Model = model,
                Color = Color
            });
        }

        public static void DrawRectUI(Matrix4x4 model, bool debug = false)
        {
            rects.Add(new DebugRect()
            {
                Model = model
            });
        }

        public static void DrawRectUIFilled(Matrix4x4 model)
        {
            rects.Add(new DebugRect()
            {
                Model = model,
                materialOverride = debugMaterial,
            });
        }

        public static void DrawRectUI(Matrix4x4 model, MaterialInstance materialOverride)
        {
            rects.Add(new DebugRect()
            {
                Model = model,
                materialOverride = materialOverride
            });
        }

        public static Matrix4x4 RectToMatrix(Vector2 pos, Vector2 size)
        {
            return
                Matrix4x4.CreateScale(size.X, size.Y, 1) *
                Matrix4x4.CreateTranslation(pos.X, pos.Y, 0);
        }

        static List<RenderItem> renderItems3D = new();
        static List<RenderItem> renderItems2D = new();

        public static void Render(CameraData cameraData, Framebuffer cameraRenderSurface)
        {
            if (!AllowDebugDraw)
            {
                ClearDebugShapes();
                return;
            }

            renderItems3D.Clear();
            renderItems2D.Clear();

            debugShaderDataBuffer.Bind(1);

            cameraRenderSurface.Bind();
            Renderer.SetupCamera(cameraData);

            for (int i = 0; i < cubes.Count; i++)
            {
                debugShaderData.Color = cubes[i].Color;

                if (CheckBufferData(ref debugShaderData, ref lastDebugShaderData))
                {
                    debugShaderDataBuffer.SetData(debugShaderData);
                }
                renderItems3D.Add(new RenderItem
                {
                    Material = debugMaterial,
                    Mesh = debugCube,
                    Model = cubes[i].Model
                });
            }

            for (int i = 0; i < meshes.Count; i++)
            {
                debugShaderData.Color = meshes[i].Color;

                if (CheckBufferData(ref debugShaderData, ref lastDebugShaderData))
                {
                    debugShaderDataBuffer.SetData(debugShaderData);
                }

                renderItems3D.Add(new RenderItem
                {
                    Material = debugMaterial,
                    Mesh = meshes[i].mesh,
                    Model = meshes[i].Model
                });
            }



            Renderer.ExecuteDrawList(renderItems3D, debug3DRenderState);

            Renderer.SetupCamera(UISystem.ScreenData);

            for (int i = 0; i < rects.Count; i++)
            {
                if (rects[i].materialOverride != null) continue;

                renderItems2D.Add(new RenderItem
                {
                    Material = debugMaterial,
                    Mesh = debugQuad,
                    Model = rects[i].Model
                });
            }

            Renderer.ExecuteDrawList(renderItems2D, debug2DWireRenderState);

            for (int i = 0; i < rects.Count; i++)
            {
                if (rects[i].materialOverride == null) continue;

                renderItems2D.Add(new RenderItem
                {
                    Material = rects[i].materialOverride,
                    Mesh = debugQuadFilled,
                    Model = rects[i].Model
                });
            }

            Renderer.ExecuteDrawList(renderItems2D, debug2DRenderState);

            ClearDebugShapes();
        }

        public static void ClearDebugShapes()
        {
            meshes.Clear();
            cubes.Clear();
            rects.Clear();
        }

        static bool CheckBufferData(ref DebugShaderData current, ref DebugShaderData last)
        {
            if (current.Color != last.Color)
            {
                last = current;
                return true;
            }

            return false;
        }
    }
}