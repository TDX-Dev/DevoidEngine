using DevoidEngine.Engine.UI.Theme;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.UI.Nodes
{
    public class IconNode : LabelNode
    {
        public override string ThemeType => "Icon";

        public IconNode(string text, float scale = 16) : base(text, scale)
        {

        }
    }
}
