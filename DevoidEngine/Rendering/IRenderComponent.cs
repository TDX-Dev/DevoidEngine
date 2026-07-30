using DevoidEngine.Core;

namespace DevoidEngine.Rendering
{
    public interface IRenderComponent
    {
        public void Collect(Camera camera, RenderView viewData);
    }
}
