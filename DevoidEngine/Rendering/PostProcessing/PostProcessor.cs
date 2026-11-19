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
        private readonly PostProcessGraph _graph = new();
        private readonly PostProcessContext _context = new();

        public void AddPass(PostProcessPass pass) => _graph.AddPass(pass);

        public void RemovePass(PostProcessPass pass) => _graph.RemovePass(pass);

        public void Resize(int width, int height) => _graph.Resize(width, height);

        public Texture Run(Renderer renderer, RenderContext renderContext, Texture sceneColor, PostProcessSettings settings)
        {
            _context.Renderer = renderer;
            _context.RenderContext = renderContext;
            _context.CommandList = renderContext.CommandList;
            _context.Settings = settings;

            return _graph.Execute(_context, sceneColor);
        }
    }
}
