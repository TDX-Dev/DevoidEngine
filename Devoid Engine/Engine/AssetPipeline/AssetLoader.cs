using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.AssetPipeline
{
    public static class AssetLoader
    {
        public static T Load<T>(string path)
        {
            var bytes = File.ReadAllBytes(path);

            if (typeof(T) == typeof(byte[]))
                return (T)(object)bytes;

            throw new Exception($"No loader for asset type {typeof(T)}");
        }
    }
}