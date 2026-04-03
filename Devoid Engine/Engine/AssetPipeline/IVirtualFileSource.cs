using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Core
{
    public interface IVirtualFileSource
    {
        bool Exists(string path);

        Stream OpenRead(string path);

        IEnumerable<string> Enumerate(string path);
    }
}
