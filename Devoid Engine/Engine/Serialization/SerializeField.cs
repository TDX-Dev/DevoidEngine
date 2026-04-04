using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Serialization
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SerializeFieldAttribute : Attribute
    {
    }
}
