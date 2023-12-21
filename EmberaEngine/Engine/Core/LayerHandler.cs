using EmberaEngine.Engine.Utilities;
using System;
using System.Collections.Generic;

namespace EmberaEngine.Engine.Core
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

        public void TextInput(int unicode)
        {
            TextInputEvent textInputEvent = new TextInputEvent()
            {
                Unicode = unicode
            };

            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].OnTextInput(textInputEvent);
            }
        }

        public void KeyDownInput(Keys key, int scanCode, string modifiers, bool caps)
        {
            KeyboardEvent keyboardEvent = new KeyboardEvent()
            {
                Key = key,
                scanCode = scanCode,
                modifiers = modifiers,
                Caps = caps
            };

            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].OnKeyDown(keyboardEvent);
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

        public void RenderLayers()
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].OnRender();
            }
        }

        public void LateRenderLayers()
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].OnLateRender();
            }
        }

        public void AddLayer(Layer layer)
        {
            layers.Add(layer);
        }

        public void RemoveLayer(Layer layer)
        {
            layers.Add(layer);
        }

    }
}
