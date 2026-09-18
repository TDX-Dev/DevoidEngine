using DevoidEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elemental.Tools.Documents
{
    public sealed class SceneDocument : EditorDocument
    {
        public Scene Scene { get; set; }
        public override string DisplayName => HasFile ? Path.GetFileNameWithoutExtension(FilePath!) : Scene.SceneName ?? "Untitled";

        public SceneDocument(Scene scene, string? filePath = null) : base(filePath)
        {
            Scene = scene;
        }
    }
}
