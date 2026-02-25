using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;
using DevoidGPU;

namespace DevoidEngine.Engine.Rendering
{
    public class RenderBatch
    {
        public Camera camera;
        public List<RenderInstance> instances;
    }

    public static class RenderAPI
    {
        private static Pool<RenderBatch> RenderBatchPool = new Pool<RenderBatch>();
        private static List<RenderBatch> RenderBatches = new List<RenderBatch>();


        //public static List<RenderBatch> BuildRenderBatches(List<IRenderCommand> commands)
        //{
        //    RenderBatch currentRenderBatch = null;

        //    foreach (IRenderCommand command in commands)
        //    {
        //        if (command is SetViewInfoCommand3D)
        //        {
        //            SetViewInfoCommand3D viewInfo3D = (SetViewInfoCommand3D)command;

        //            currentRenderBatch = RenderBatchPool.Get();
        //            currentRenderBatch.instances = new List<RenderInstance>();

        //            currentRenderBatch.camera = viewInfo3D.camera;

        //            EnginePipeline.SetViewInfoPool.Return(ref viewInfo3D);

        //        }
        //        else if (command is DrawMeshIndexed)
        //        {
        //            DrawMeshIndexed drawMeshIndexed = (DrawMeshIndexed)command;

        //            currentRenderBatch.instances.Add(drawMeshIndexed.instance);

        //            EnginePipeline.DrawMeshIndexedPool.Return(ref drawMeshIndexed);
        //        }
        //        else if (command is Render3DStateCommand)
        //        {
        //            Render3DStateCommand render3DStartCommand = (Render3DStateCommand)command;

        //            if (currentRenderBatch.camera == null)
        //                throw new InvalidOperationException("Cannot call render commands without active camera");

        //            if (render3DStartCommand.state == RendererStateCommnandType.End)
        //            {
        //                RenderBatches.Add(currentRenderBatch);
        //                currentRenderBatch = RenderBatchPool.Get();
        //            }

        //            EnginePipeline.Renderer3DStateCommand.Return(ref render3DStartCommand);
        //        }
        //    }

        //    return RenderBatches;
        //}

        //public static void ProcessCommands(List<IRenderCommand> commands)
        //{
        //    List<RenderBatch> renderBatches =  BuildRenderBatches(commands);

        //    for (int i = 0; i < renderBatches.Count; i++)
        //    {
        //        RenderBatch renderBatch = renderBatches[i];

        //        Renderer3D.BeginRender(renderBatch.camera);

        //        Renderer3D.Render(renderBatch.instances);

        //        Renderer3D.EndRender();

        //        RenderBatchPool.Return(ref renderBatch);
        //    }

        //    RenderBatches.Clear();
        //}

        static RenderAPI()
        {
            mesh = new Mesh();
            mesh.SetVertices(Primitives.GetScreenQuadVertex());
        }

        static Mesh mesh;


        // This method should only be called at the end of the render stage
        public static void RenderToScreen(Texture2D texture)
        {
            if (texture == null) return;
            Renderer.graphicsDevice.MainSurface.Bind();

            Renderer.graphicsDevice.SetRasterizerState(CullMode.None);
            Renderer.graphicsDevice.SetPrimitiveType(PrimitiveType.Triangles);

            IInputLayout inputLayout = Renderer.GetInputLayout(mesh, ShaderLibrary.GetShader("Screen/RENDER_SCREEN"));

            inputLayout.Bind();
            mesh.Bind();

            ShaderLibrary.GetShader("Screen/RENDER_SCREEN").Use();

            texture.BindSampler(0);
            texture.Bind(0);

            Renderer.graphicsDevice.Draw(mesh.GetVertices().Length, 0);

            Renderer.graphicsDevice.UnbindAllShaderResources();

        }

    }
}
