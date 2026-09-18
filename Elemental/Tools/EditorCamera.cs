using DevoidEngine.Core;

namespace Elemental.Tools
{
    public class EditorCamera
    {
        public Camera Camera { get; set; }
        public bool CanInteract = false;
        public bool IsInteracting = false;

        public EditorCamera()
        {
            Camera = new Camera();
        }


        public void Update(EditorContext context, float deltaTime)
        {




        }
    }
}
