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

        public void Resize(int width, int height)
        {
            graph.Resize(width, height);
        }

        public Texture2D Run(Texture2D input)
        {
            return graph.Execute(input);
        }
    }
}