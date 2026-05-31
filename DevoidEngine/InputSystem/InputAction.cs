using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.InputSystem
{
    public class InputAction
    {
        public string Name { get; set; } = "";
        public List<InputBinding> Bindings { get; set; }

        public InputAction()
        {
            Bindings = [];
        }
    }
}
