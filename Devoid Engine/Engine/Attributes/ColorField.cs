using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ColorField : Attribute
    {
        public bool HDR;

        public ColorField(bool hdr = false)
        {
            HDR = hdr;
        }
    }
}
