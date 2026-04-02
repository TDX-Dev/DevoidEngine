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

        public override void OnStart()
        {
            Renderer.SkyboxRenderer.SetPanorama(Helper.LoadHDRI("Engine/Content/HDRIs/qwantani_dusk_2_puresky_4k.hdr"));
        }

        public override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);
        }
    }
}
