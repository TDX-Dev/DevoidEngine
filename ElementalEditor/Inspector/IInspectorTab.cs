using DevoidEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Inspector
{
    public interface IInspectorTab
    {
        string Id { get; }
        string Icon { get; }
        Vector4 IconColor { get; }

        void Draw(EditorContext context, GameObject obj);
    }
}
