using DevoidEngine.Core;
using DevoidGPU;

namespace DevoidEngine.Rendering
{
    public struct RenderContext
    {
        public Renderer Renderer;
        public Viewport Viewport;
        public Camera Camera;

        public RenderResourceCache Resources;

        public ICommandList CommandList;

        public Texture SceneDepth;
        public Texture SceneNormal;
        public Texture SceneAO;
    }
}
