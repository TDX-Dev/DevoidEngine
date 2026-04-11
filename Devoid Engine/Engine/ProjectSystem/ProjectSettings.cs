using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.ProjectSystem
{
    public class ProjectSettings
    {
        public int RenderWidth { get; set; } = 1280;
        public int RenderHeight { get; set; } = 720;
        public string StartupScene { get; set; } = "";

        // Physics Related Stuff
        public bool UsePhysicsInterpolation { get; set; } = true;
        public int PhysicsUpdateFrequency { get; set; } = 60;

    }
}
