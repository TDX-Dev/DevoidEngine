using DevoidEngine.Engine.Core;
using DevoidGPU.DX11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidStandaloneLauncher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ApplicationSpecification applicationSpecification = new ApplicationSpecification()
            {
                darkTitlebar = true,
                forceVsync = true,
                graphicsDevice = new DX11GraphicsDevice(),
                Width = 1280,
                Height = 720,
                useFullscreen = false,
                useImGui = true,
                useDebugConsole = true,
                Name = "Devoid New Beginnings"
            };


            Console.WriteLine("Runtime Started");
            Application application = new Application();
            application.Initialize(applicationSpecification);
            application.TargetFrameRate = 10;
            EngineSingleton.Instance.UseInterpolation = true;
            application.AddLayer(new PrototypeLoader());
            application.Run();
        }


    }
}
