using DevoidEngine.InputSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Core
{
    public sealed class ProjectSettings
    {
        public GameSettings Game { get; set; } = new();
        public InputSettings Input { get; set; } = new();
        public PhysicsSettings Physics { get; set; } = new();
        //public RenderingSettings Rendering { get; set; } = new();
    }

    public sealed class GameSettings
    {
        public string StartupScene { get; set; } = string.Empty;
        public int TargetFrameRate { get; set; } = 165;
    }

    public sealed class PhysicsSettings
    {
        public float FixedTimeStep { get; set; } = 1.0f / 60.0f;
        public bool EnablePhysics { get; set; } = true;
    }

    public sealed class InputSettings
    {
        public List<InputAction> Actions { get; set; } = [];
    }
}
