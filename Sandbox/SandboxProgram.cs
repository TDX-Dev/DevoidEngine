using DevoidEngine.Core;
using DevoidEngine.InputSystem;
using DevoidEngine.InputSystem.InputDevices;
using DevoidEngine.Util;
using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
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
            Console.WriteLine("Sandbox has launched.");

            mesh = PrimitiveMeshes.GetCube();

            shader = Shader.FromDescriptorFile(Engine.GraphicsDevice, "Content/DevoidShaderDescriptors/basic.dsd");

            ubo = new UniformBuffer(Engine.GraphicsDevice, ResourceUsage.Dynamic, 4);

            layout = Engine.GraphicsDevice.CreateDescriptorLayout(
            [
                new DescriptorBinding()
                {
                    Binding = 1,
                    Stages = DevoidGPU.ShaderStage.Fragment,
                    Type = DescriptorType.UniformBuffer
                }
            ]);

            set = Engine.GraphicsDevice.CreateDescriptorSet(layout);

            set.SetUniformBuffer(1, ubo.GPU);

            ubo.GPU.Update<uint>([64]);


            scene = new Scene();
            Engine.Instance.SceneManager.LoadScene(scene);

            scene.AddGameObject("Hello World");


            Engine.InputSystem.AddBinding("Hello", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.K,
                IsClamped = true
            });
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
            cmd.SetViewport(0, 0, 50, 100);
            cmd.SetPipeline(shader.GetPass("Forward").GetPipeline(Engine.GraphicsDevice, Vertex.VertexInfo));
            cmd.SetDescriptorSet(0, set);
            mesh.Draw(cmd);

        }
    }
}
