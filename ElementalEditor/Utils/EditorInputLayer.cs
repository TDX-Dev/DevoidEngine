using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Utils
{
    public class EditorInputLayer : IInputLayer
    {
        EditorCamera camera;

        public EditorInputLayer(EditorCamera cam)
        {
            camera = cam;
        }

        public bool Handle(ref InputEvent e)
        {
            if (e.DeviceType == InputDeviceType.Mouse)
            {
                camera.HandleMouse(e);
                return true;
            }

            return false;
        }
    }
}
