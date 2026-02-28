using DevoidEngine.Engine.Core;
using DevoidGPU.DX11;


namespace DevoidStandaloneLauncher
{
    internal class Program
    {
        static ApplicationSpecification applicationOptions = new ApplicationSpecification()
        {
            Height = 720,
            Width = 1280,
            graphicsDevice = new DX11GraphicsDevice(),
            forceVsync = true,
            useImGui = true,

            Name = "LockedIn - Devoid"
        };


        static Application baseApplication;


        static void InitializeEngine()
        {
            baseApplication = new Application();
            baseApplication.Create(applicationOptions);
        }



        static void Main(string[] args)
        {
            Console.WriteLine("Initializing Sandbox");
            InitializeEngine();

            PrototypeLoader baseGame = new PrototypeLoader();
            baseApplication.AddLayer(baseGame);

            baseApplication.Run();

        }
    }
}
