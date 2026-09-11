using DevoidEngine.InputSystem.InputDevices;
using DevoidGPU;

namespace DevoidEngine.Core
{
    public class LayerManager
    {
        public List<Layer> layers;

        public LayerManager()
        {
            layers = [];
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

        public void RenderLayers(ICommandList cmd)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].OnRender(cmd);
            }
        }

        public void OnGUILayers()
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].OnGUIRender();
            }
        }

        public void PostRenderLayers(ICommandList cmd)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].OnPostRender(cmd);
            }
        }

        public void KeyDownLayers(Keys keys, int scancode, KeyModifiers modifiers, bool isRepeated)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].OnKeyDown(keys, scancode, modifiers, isRepeated);
            }
        }

        public void KeyUpLayers(Keys keys, int scancode, KeyModifiers modifiers, bool isRepeated)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].OnKeyUp(keys, scancode, modifiers, isRepeated);
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
