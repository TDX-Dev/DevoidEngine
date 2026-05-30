using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

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

        public void PostRenderLayers()
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
