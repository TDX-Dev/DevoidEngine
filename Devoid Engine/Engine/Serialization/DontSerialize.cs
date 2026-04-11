using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Serialization
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class DontSerialize : Attribute
    {
    }
}
