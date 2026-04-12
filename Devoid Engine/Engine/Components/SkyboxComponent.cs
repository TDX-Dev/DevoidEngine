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
        internal Texture2D? hdriTexture;

        public Texture2D? HDRIImage
        {
            get => hdriTexture;
            set
            {
                if (value == null)
                    return;
                hdriTexture = value;
                Renderer.SkyboxRenderer.SetPanorama(value!);
            }
        }

        public override void OnStart()
        {
            if (hdriTexture != null)
                Renderer.SkyboxRenderer.SetPanorama(hdriTexture);
        }

        public override void OnDestroy()
        {
            //HDRIImage?.Dispose();
        }

        public override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);
        }
    }
}
