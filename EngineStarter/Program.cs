using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Imgui;

namespace EngineStarter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Application app = new Application();

            ApplicationSpecification specification = new ApplicationSpecification()
            {
                Name = "Hello World",
                Height = 600,
                Width = 800,
                forceVsync = true,
                //useImGui = true,
            };

            app.Create(specification);

            //ImguiLayer.UseDockspace = true;

            DebugLayer debug = new DebugLayer();

            debug.Application = app;

            app.AddLayer(debug);

            app.Run();
        }
    }
}