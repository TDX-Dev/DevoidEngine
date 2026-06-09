using DevoidEngine.Components;
using DevoidEngine.Core;
using DevoidEngine.InputSystem;
using DevoidEngine.InputSystem.InputDevices;
using DevoidEngine.Util;
using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
    struct Material
    {
        public Vector4 Albedo;
    }

    internal class SandboxProgram : Layer
    {

        Scene scene = null!;

        public override void OnAttach()
        {
            Application.MainWindow.OnResize += MainWindow_OnResize;
            Engine.Instance.SceneTree.OnSceneChanged += SceneTree_OnSceneChanged;

            Engine.Instance.SceneTree.RootViewport.Resize(Application.MainWindow.Window.Size.X, Application.MainWindow.Window.Size.Y);

            Console.WriteLine("Sandbox has launched.");

            scene = new Scene();
            Engine.Instance.SceneTree.LoadScene(scene);

            go = scene.AddGameObject("Hello World");
            
            go.AddComponent<Camera3D>();

            meshGo = scene.AddGameObject("Hello Mesh Object");
            meshGo.AddComponent<MeshRenderer>();

            Engine.InputSystem.AddBinding("Hello", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.K,
                IsClamped = true
            });
        }

        GameObject go;
        GameObject meshGo;

        private void SceneTree_OnSceneChanged(Scene obj)
        {
            Console.WriteLine("Scene changed");
            Application.MainWindow.Window.Title = obj.SceneName + " - Devoid Engine";
        }

        private void MainWindow_OnResize(int width, int height)
        {
            Engine.Instance.SceneTree.RootViewport.Resize(width, height);
        }

        public override void OnDetach()
        {
            Console.WriteLine("Sandbox exited successfully");
        }

        public override void OnUpdate(float deltaTime)
        {
            if (Engine.InputSystem.GetActionDown("Hello"))
            {
                Console.WriteLine("K was pressed");
            }
            meshGo.Transform.Position = new Vector3(0, 0, meshGo.Transform.Position.Z + deltaTime);
        }

        public override void OnRender(ICommandList cmd)
        {
            //cmd.SetViewport(0, 0, Application.MainWindow.Window.Size.X, Application.MainWindow.Window.Size.Y);
            ////cmd.SetPipeline(shader.GetPass("Forward").GetPipeline(Engine.GraphicsDevice, Vertex.VertexInfo));
            ////cmd.SetDescriptorSet(1, set);
            ////mesh.Draw(cmd);

        }

        public override void OnPostRender(ICommandList cmd)
        {
            cmd.SetFramebuffer(Application.MainWindow.Framebuffer);
            Engine.Renderer.API.RenderToScreen(cmd, Engine.Instance.SceneTree.RootViewport.OutputTexture!);
        }
    }
}
