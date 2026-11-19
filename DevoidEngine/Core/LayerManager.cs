using DevoidEngine.InputSystem.InputDevices;
using DevoidGPU;

namespace DevoidEngine.Core
{
    public class LayerManager
    {
        public List<Layer> Layers;

        public LayerManager()
        {
            Layers = [];
        }

        public void AttachLayers()
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].OnAttach();
            }
        }

        public void DetachLayers()
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].OnDetach();
            }
        }

        public void ResizeLayers(int width, int height)
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].OnResize(width, height);
            }
        }

        public void UpdateLayers(float dt)
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].OnUpdate(dt);
            }
        }

        public void FixedUpdateLayers(float dt)
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].OnFixedUpdate(dt);
            }
        }

        public void RenderLayers(ICommandList cmd)
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].OnRender(cmd);
            }
        }

        public void OnGUILayers()
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].OnGUIRender();
            }
        }

        public void PostRenderLayers(ICommandList cmd)
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].OnPostRender(cmd);
            }
        }

        public void KeyDownLayers(Keys keys, int scancode, KeyModifiers modifiers, bool isRepeated)
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].OnKeyDown(keys, scancode, modifiers, isRepeated);
            }
        }

        public void KeyUpLayers(Keys keys, int scancode, KeyModifiers modifiers, bool isRepeated)
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].OnKeyUp(keys, scancode, modifiers, isRepeated);
            }
        }

        public void AddLayer(Layer layer)
        {
            Layers.Add(layer);
        }

        public void RemoveLayer(Layer layer)
        {
            Layers.Remove(layer);
        }
    }
}
