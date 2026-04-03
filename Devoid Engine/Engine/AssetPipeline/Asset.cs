using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.AssetPipeline
{
    public static class Asset
    {
        public static T Load<T>(string path)
        {
            Guid guid = AssetDatabase.GetGuid(path);
            return AssetManager.Load<T>(guid);
        }
    }
}
