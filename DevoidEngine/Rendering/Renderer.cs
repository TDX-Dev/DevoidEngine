using DevoidEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Rendering
{
    public enum RenderTechnique
    {
        Forward,
        Clustered
    }

    public struct RendererConfig {
        public RenderTechnique Technique;
    }

    public sealed class Renderer : IDisposable
    {
        public IRenderTechnique? ActiveTechnique { get; set; }

        public ShaderLibrary ShaderLibrary { get; set; } = null!;

        public RenderWorld World { get; private set; } = null!;

        public Material DefaultMaterial { get; private set; } = null!;

        public void Initialize(RendererConfig config)
        {
            if (Engine.GraphicsDevice == null)
                throw new Exception("Graphics device not initialized yet.");

            ActiveTechnique = config.Technique switch
            {
                RenderTechnique.Forward => new ForwardRenderTechnique(),
                _ => throw new NotImplementedException(nameof(config.Technique) + " is not implemented."),
            };

            ShaderLibrary = new ShaderLibrary();
            World = new RenderWorld();
        


        }




        public void Dispose()
        {
            ActiveTechnique?.Dispose();
        }


    }
}
