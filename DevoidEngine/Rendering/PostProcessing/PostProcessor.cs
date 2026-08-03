using DevoidEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Rendering.PostProcessing
{
    public sealed class PostProcessor
    {
        private readonly PostProcessGraph graph = new();
        private readonly PostProcessContext context = new();

        public void AddPass(PostProcessPass pass)
            => graph.AddPass(pass);

        public void RemovePass(PostProcessPass pass)
            => graph.RemovePass(pass);

        public void Resize(int width, int height)
            => graph.Resize(width, height);

        public Texture Run(
            Renderer renderer,
            RenderContext renderContext,
            Texture sceneColor)
        {
            context.Renderer = renderer;
            context.RenderContext = renderContext;
            context.CommandList = renderContext.CommandList;

            return graph.Execute(context, sceneColor);
        }
    }
}
