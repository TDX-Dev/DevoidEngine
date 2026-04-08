using DevoidEngine.Engine.Core;

namespace DevoidEngine.Engine.Rendering.PostProcessing
{

    public class PostProcessor
    {
        RenderGraph graph = new();

        public void AddPass(RenderGraphPass pass)
        {
            graph.AddPass(pass);
        }

        public void RemovePass(RenderGraphPass pass)
        {
            graph.RemovePass(pass);
        }

        public T? GetPass<T>() where T : RenderGraphPass
        {
            var passes = graph.GetPasses();
            foreach (var pass in passes)
            {
                if (pass is T postprocessPass)
                    return postprocessPass;
            }
            return null;
        }

        public void Resize(int width, int height)
        {
            graph.Resize(width, height);
        }

        public Texture2D Run(Texture2D sceneColor, CameraRenderContext ctx)
        {
            return graph.Execute(sceneColor, ctx);
        }
    }
}