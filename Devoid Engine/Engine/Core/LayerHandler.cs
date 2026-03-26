using DevoidEngine.Engine.Utilities;

namespace DevoidEngine.Engine.Core
{
    public class LayerHandler
    {

        public List<Layer> layers;

        public LayerHandler()
        {
            layers = new List<Layer>();
        }

        public void AttachLayers()
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].OnAttach();
            }
        }

        public void DetachLayers()
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].OnDetach();
            }
        }


        public void ResizeLayers(int width, int height)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].OnResize(width, height);
            }
        }

        public void UpdateLayers(float dt)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].OnUpdate(dt);
            }
        }

        public void FixedUpdateLayers(float dt)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].OnFixedUpdate(dt);
            }
        }

        public void RenderLayers()
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].OnRender();
            }
        }

        public void OnGUILayers()
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].OnGUIRender();
            }
        }

        public void LateRenderLayers()
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].OnPostRender();
            }
        }

        public void AddLayer(Layer layer)
        {
            layers.Add(layer);
        }

        public void RemoveLayer(Layer layer)
        {
            layers.Remove(layer);
        }

    }
}