using DevoidEngine.Core;

namespace DevoidEngine.Rendering
{
    public interface IRenderTechnique : IDisposable
    {
        void Initialize();
        RenderTarget Render(RenderContext ctx, RenderView view);
    }
}
