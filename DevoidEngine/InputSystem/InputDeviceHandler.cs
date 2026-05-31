using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.InputSystem
{
    public abstract class InputDeviceHandler
    {
        public abstract void Register(InputBackend backend);
        public abstract void Update(InputBackend backend);
    }
}
