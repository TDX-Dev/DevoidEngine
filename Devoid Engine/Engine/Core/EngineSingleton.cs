using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Core
{
    public class EngineSingleton
    {
        public static EngineSingleton Instance { get; private set; }

        public EngineSingleton()
        {
            if (Instance != null)
                throw new Exception("Cannot create multiple engine singletons");

            Instance = this;
        }

    }
}
