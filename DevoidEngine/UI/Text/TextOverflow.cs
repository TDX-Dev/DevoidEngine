using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.UI.Text
{
    public enum TextOverflow
    {
        None,       // Unlimited width
        Wrap,       // Break into multiple lines
        Clip,       // Stop rendering beyond MaxWidth
        Ellipsis    // Replace end with ...
    }
}
