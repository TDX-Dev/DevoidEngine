using DevoidEngine.Engine.AssetPipeline;
using DevoidEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidStandaloneLauncher.Prototypes
{
    public class ProjectLoaderTest : Prototype
    {
        public override void OnInit()
        {
            Texture2D shrekTexture = Asset.Load<Texture2D>("shrk.png");
        }

    }
}
