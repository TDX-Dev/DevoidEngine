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
        Mesh mesh = null!;
        Shader shader = null!;
        UniformBuffer ubo = null!;
        IDescriptorLayout layout = null!;
        IDescriptorSet set = null!;

        Scene scene = null!;

        public override void OnAttach()
        {
            Application.MainWindow.OnResize += MainWindow_OnResize;
            Engine.Instance.SceneTree.OnSceneChanged += SceneTree_OnSceneChanged;

            Console.WriteLine("Sandbox has launched.");

            mesh = PrimitiveMeshes.GetCube();

            shader = Shader.FromDescriptorFile(Engine.GraphicsDevice, "Content/DevoidShaderDescriptors/basic.dsd");

            ubo = new UniformBuffer(Engine.GraphicsDevice, ResourceUsage.Dynamic, 4);
            ubo.Update<Material>(new Material()
            {
                Albedo = new Vector4(0, 1, 0, 1)
            });


            layout = Engine.GraphicsDevice.CreateDescriptorLayout(
            [
                new DescriptorBinding()
                {
                    Binding = 0,
                    Stages = DevoidGPU.ShaderStage.Fragment,
                    Type = DescriptorType.UniformBuffer
                }
            ]);

            set = Engine.GraphicsDevice.CreateDescriptorSet(layout);
            set.SetUniformBuffer(1, ubo.GPU);

            scene = new Scene();
            Engine.Instance.SceneTree.LoadScene(scene);

            go = scene.AddGameObject("Hello World");
            go.AddComponent<MeshRenderer>();

            Engine.InputSystem.AddBinding("Hello", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.K,
                IsClamped = true
            });
        }

        GameObject go;

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
        }

        public override void OnRender(ICommandList cmd)
        {
            cmd.SetViewport(0, 0, Application.MainWindow.Window.Size.X, Application.MainWindow.Window.Size.Y);
            cmd.SetPipeline(shader.GetPass("Forward").GetPipeline(Engine.GraphicsDevice, Vertex.VertexInfo));
            cmd.SetDescriptorSet(0, set);
            mesh.Draw(cmd);

        }
    }
}
