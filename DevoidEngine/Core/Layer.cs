using DevoidEngine.InputSystem.InputDevices;
using DevoidGPU;

namespace DevoidEngine.Core
{
    public class Layer
    {
        // The application to which the layer belongs to.
        public Application Application { get; internal set; } = null!;

        // This method is called when the layer is attached to the application
        public virtual void OnAttach() { }
        // This method is called when the layer is removed from the application
        public virtual void OnDetach() { }

        public virtual void OnFixedUpdate(float deltaTime) { }
        // This method is called every frame to update the logic of the application
        public virtual void OnUpdate(float deltaTime) { }
        public virtual void OnPreRender(ICommandList cmd) { }
        // This method is called before the renderer commences rendering, this is for submitting the meshes to be rendered to the renderer
        public virtual void OnRender(ICommandList cmd) { }
        // This method is called once all the rendering is done
        public virtual void OnPostRender(ICommandList cmd) { }
        // This method is called whenever the application window is resized
        public virtual void OnResize(int width, int height) { }
        // This method is called before the imgui renderer commences rendering
        public virtual void OnGUIRender() { }
        public virtual void OnKeyDown(Keys keys, int Scancode, KeyModifiers modifiers, bool isRepeated) { }
        public virtual void OnKeyUp(Keys keys, int Scancode, KeyModifiers modifiers, bool isRepeated) { }
    }
}
