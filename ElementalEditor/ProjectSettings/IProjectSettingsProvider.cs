using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.ProjectSettings
{
    public interface IProjectSettingsProvider
    {
        string Category { get; }     // "Rendering", "Physics"
        string Name { get; }         // "Graphics", "Lighting"

        void Draw();                 // Draw settings UI
    }
}
