using DevoidEngine.Core;
using DevoidEngine.Rendering.RGraph;
using DevoidEngine.Rendering.RGraph.TestPasses;
using DevoidGPU;

namespace DevoidEngine.Rendering
{
    public struct ViewportRect
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
    }
    public struct ScissorRect
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
    }

    public enum RenderTechnique
    {
        Forward
    }

    public struct RendererConfig
    {
        public RenderTechnique Technique;
    }

    public class Renderer
    {
        public const uint MAX_SPOT_LIGHTS = 32;
        public const uint MAX_POINT_LIGHTS = 32;
        public const uint MAX_DIRECTIONAL_LIGHTS = 1;

        public RenderTechnique Technique;
        public IRenderTechnique RenderTechniqueImpl = null!;

        public RenderAPI API = null!;

        private readonly Stack<ViewportRect> _viewportStack = [];
        private readonly Stack<ScissorRect> _scissorStack = [];
        private ViewportRect _currentViewport;
        private ScissorRect _currentScissor;

        // ---TESTING---
        private RenderGraph graph = null!;

        public void Initialize(RendererConfig config)
        {
            Technique = config.Technique;
            API = new();

            graph = new();
            graph.AddPass(new TestDebugPreviewPass());
            graph.AddPass(new TestShadowMapPass());
            graph.AddPass(new TestGbufferPass());
            graph.AddPass(new TestLightingPass());
            graph.AddPass(new TestBloomPass());
            graph.AddPass(new TestCompositePass());

            graph.RegisterPersistentResource("Backbuffer", Texture.Default);

            graph.Compile();
        }

        public void SetupFrame(uint frameCount)
        {

        }

        public void EndFrame()
        {

        }

        public void RenderViewport(Viewport viewport)
        {

        }

        public void Dispose()
        {

        }
        #region RASTERIZERSTATE
        public void PushViewport(ICommandList cmd, ViewportRect viewport)
        {
            _viewportStack.Push(_currentViewport);
            _currentViewport = viewport;
            cmd.SetViewport(viewport.X, viewport.Y, viewport.Width, viewport.Height);
        }
        public void PopViewport(ICommandList cmd, bool setPrevious = true)
        {
            if (_viewportStack.Count == 0)
                return;
            _currentViewport = _viewportStack.Pop();
            if (setPrevious)
                cmd.SetViewport(_currentViewport.X, _currentViewport.Y, _currentViewport.Width, _currentViewport.Height);
        }
        public void PushScissor(ICommandList cmd, ScissorRect scissor)
        {
            _scissorStack.Push(_currentScissor);
            _currentScissor = scissor;
            cmd.SetScissor(scissor.X, scissor.Y, scissor.Width, scissor.Height);
        }
        public void PopScissor(ICommandList cmd, bool setPrevious = true)
        {
            if (_scissorStack.Count == 0)
                return;
            _currentScissor = _scissorStack.Pop();
            if (setPrevious)
                cmd.SetScissor(_currentScissor.X, _currentScissor.Y, _currentScissor.Width, _currentScissor.Height);
        }
        #endregion
    }
}
