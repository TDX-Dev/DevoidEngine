using DevoidEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
    internal class SandboxProgram : Layer
    {
        public override void OnAttach()
        {
            Console.WriteLine("Sandbox has launched.");
        }

        public override void OnDetach()
        {
            Console.WriteLine("Sandbox exited successfully");
        }

        public override void OnUpdate(float deltaTime)
        {
            Console.WriteLine(1/deltaTime);
        }
    }
}
