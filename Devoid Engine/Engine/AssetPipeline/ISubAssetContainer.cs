using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.AssetPipeline
{
    public interface ISubAssetContainer
    {
        bool TryGetSubAsset<T>(ulong id, out T asset) where T : class?;
    }
}
