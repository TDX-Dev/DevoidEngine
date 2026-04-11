using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.InputSystem
{
    public class InputAction
    {
        public string Name { get; set; } = "";
        public List<InputBinding> Bindings { get; set; } = new();
    }
}
