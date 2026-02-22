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

        public void KeyUpInput(Keys key, int scanCode, string modifiers, bool caps)
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
                layers[i].OnKeyUp(keyboardEvent);
            }
        }

        public void OnMouseMoveEvent(OpenTK.Windowing.Common.MouseMoveEventArgs obj)
        {
            MouseMoveEvent mouseMoveEvent = new MouseMoveEvent()
            {
                position = TypeHelper.ToNumerics2(obj.Position),
                delta = TypeHelper.ToNumerics2(obj.Delta),
            };

            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].OnMouseMove(mouseMoveEvent);
            }
        }

        public void OnMouseWheelEvent(OpenTK.Windowing.Common.MouseWheelEventArgs obj)
        {
            MouseWheelEvent mouseWheelEvent = new MouseWheelEvent()
            {
                Offset = TypeHelper.ToNumerics2(obj.Offset),
            };

            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].OnMouseWheel(mouseWheelEvent);
            }
        }

        public void OnMouseButtonEvent(OpenTK.Windowing.Common.MouseButtonEventArgs obj)
        {
            MouseButtonEvent mouseBtnEvent = new MouseButtonEvent()
            {
                Button = (MouseButton)obj.Button,
                Action = (InputAction)obj.Action,
                Modifiers = (KeyModifiers)obj.Modifiers,
            };

            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].OnMouseButton(mouseBtnEvent);
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

        public void RenderLayers(float deltaTime)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].OnRender(deltaTime);
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
                layers[i].OnLateRender();
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
