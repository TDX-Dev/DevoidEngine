using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Components
{
    public class SkyboxComponent : Component
    {
        public override string Type => nameof(SkyboxComponent);

        Texture2D HdriImage = null!;

        public override void OnStart()
        {
            HdriImage = Helper.LoadHDRI("Engine/Content/HDRIs/qwantani_dusk_2_puresky_4k.hdr");
            Renderer.SkyboxRenderer.SetPanorama(HdriImage);
            HdriImage.Dispose();
        }

        public override void OnDestroy()
        {
            HdriImage?.Dispose();
        }

        public override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);
        }
    }
}
