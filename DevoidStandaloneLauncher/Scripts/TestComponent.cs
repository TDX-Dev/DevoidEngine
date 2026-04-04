using DevoidEngine.Engine.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidStandaloneLauncher.Scripts
{
    public class TestComponent : Component
    {
        public override string Type => "TestComponent";
        public float Health = 100;
        public Vector3 Position = new(1, 2, 3);
        public Dictionary<string, Vector3> KeyVecs = new Dictionary<string, Vector3>()
        {
            {"Hello", new Vector3(0, 1, 0) },
            {"Bye", new Vector3(0, 0, 1) },
            {"Never gonna give you up.", new Vector3(0, -1, 0) }
        };
    }
}
