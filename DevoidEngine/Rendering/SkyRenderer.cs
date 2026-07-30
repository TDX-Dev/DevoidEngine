using System.Numerics;

namespace DevoidEngine.Rendering
{
    public class SkyRenderer
    {
        public ISky Sky = null!;

        public EnvironmentLighting Environment;

        readonly RenderMeshData skyMeshData;

        public SkyRenderer()
        {
            Environment = new();
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

            if (Sky.Dirty)
            {
                Sky.BuildEnvironment(ctx);

                ProcessCubemap(Sky);
            }
            
        }

        void ProcessCubemap(ISky sky)
        {

        }

        //public void Render(RenderContext ctx)
        //{
        //    if (Sky == null)
        //        return;

        //    skyMeshData.render_transform = Matrix4x4.CreateScale(2) * Matrix4x4.CreateTranslation(ctx.Camera.Position);

        //    ctx.Renderer.Execute(ctx.CommandList, skyMeshData);
        //}
    }
}
