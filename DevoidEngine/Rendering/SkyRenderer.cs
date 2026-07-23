using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Rendering
{
    public class SkyRenderer
    {
        public ISky Sky = null!;

        readonly RenderMeshData skyMeshData;

        public SkyRenderer()
        {
            Sky = new ProceduralSky();

            skyMeshData = new RenderMeshData
            {
                render_mesh = Sky.Mesh,
                render_material = Sky.Material,
            };
        }

        public void Render(RenderContext ctx)
        {
            if (Sky == null)
                return;

            skyMeshData.render_transform = Matrix4x4.CreateScale(2) * Matrix4x4.CreateTranslation(ctx.Camera.Position);

            ctx.Renderer.Execute(ctx.CommandList, skyMeshData);
        }
    }
}
