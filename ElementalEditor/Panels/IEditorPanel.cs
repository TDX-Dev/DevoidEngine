using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Panels
{
    public interface IEditorPanel
    {
        void Draw(EditorContext context);
    }
}
