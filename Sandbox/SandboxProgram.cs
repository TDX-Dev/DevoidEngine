using DevoidEngine.AssetPipeline;
using DevoidEngine.Audio;
using DevoidEngine.Components;
using DevoidEngine.Core;
using DevoidEngine.InputSystem;
using DevoidEngine.InputSystem.InputDevices;
using DevoidEngine.Rendering;
using DevoidEngine.Util;
using DevoidGPU;
using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{

    internal class SandboxProgram : Layer
    {

        Scene scene = null!;

        MaterialInstance PBRMaterial = null!;
        MaterialInstance GroundPBRMaterial = null!;

        readonly Random rand = new();

        public override void OnAttach()
        {
            Application.MainWindow.OnResize += MainWindow_OnResize;
            Engine.Instance.SceneTree.OnSceneChanged += SceneTree_OnSceneChanged;

            Engine.Instance.SceneTree.RootViewport.Resize(Application.MainWindow.Window.Size.X, Application.MainWindow.Window.Size.Y);

            Console.WriteLine("Sandbox has launched.");

            scene = new Scene();
            Engine.Instance.SceneTree.LoadScene(scene);
            scene.Play();

            PBRMaterial = new MaterialInstance(Engine.Renderer.DefaultMaterial);
            GroundPBRMaterial = new MaterialInstance(Engine.Renderer.DefaultMaterial);
            PBRMaterial.SetVector4("Albedo", new Vector4(1, 1, 1, 1));
            GroundPBRMaterial.SetVector4("Albedo", new Vector4(1, 1, 1, 1));

            Texture dvsTex = Asset.Load<Texture>("mesh_tex.png")!;
            //Texture dvsTex = Texture.CreateFromImage2D(TextureUtil.LoadImage("Assets/mesh_tex.png"), TextureUsage.ShaderResource);
            Texture terrTex = Texture.CreateFromImage2D(TextureUtil.LoadImage("Assets/ground_tex.png"), TextureUsage.ShaderResource);

            GroundPBRMaterial.SetTexture("MAT_AlbedoMap", terrTex);
            GroundPBRMaterial.SetFloat("Roughness", 0.2f);


            PBRMaterial.SetTexture("MAT_AlbedoMap", dvsTex);
            PBRMaterial.SetFloat("Roughness", 0.2f);

            go = scene.AddGameObject("Hello World");
            
            cam = go.AddComponent<Camera3D>();

            meshGo = scene.AddGameObject("Hello Mesh Object");
            MeshRenderer meshRenderer = meshGo.AddComponent<MeshRenderer>();
            meshRenderer.Mesh = PrimitiveMeshes.GetCube();
            meshRenderer.Material = GroundPBRMaterial;
            meshGo.Transform.Position = new Vector3(0, 0, -10);
            meshGo.Transform.Scale = new Vector3(20, 1f, 20);
            meshGo.Transform.EulerAngles = new Vector3(10, 0, 0);
            StaticColliderComponent sb = meshGo.AddComponent<StaticColliderComponent>();
            sb.Shape = new DevoidEngine.Physics.PhysicsShapeDescription()
            {
                Type = DevoidEngine.Physics.PhysicsShapeType.Box,
                Size = new Vector3(20, 1f, 20),
                
            };

            meshGo1 = scene.AddGameObject("Hello Mesh 2 Object");
            MeshRenderer meshRenderer1 = meshGo1.AddComponent<MeshRenderer>();
            meshRenderer1.Mesh = PrimitiveMeshes.GetCube();
            meshRenderer1.Material = PBRMaterial;
            meshGo1.Transform.Position = new Vector3(0, 10, 0);
            RigidBodyComponent rb = meshGo1.AddComponent<RigidBodyComponent>();

            GameObject childObject = scene.AddGameObject("PhyChildObj");
            MeshRenderer meshRenderer2 = childObject.AddComponent<MeshRenderer>();
            meshRenderer2.Mesh = PrimitiveMeshes.GetCube();
            meshRenderer2.Material = PBRMaterial;
            childObject.Transform.SetParent(meshGo1.Transform);
            childObject.Transform.LocalPosition = new Vector3(0, 2, -2);

            //rb.Shape = new DevoidEngine.Physics.PhysicsShapeDescription()
            //{
            //    Radius = 1,
            //    Type = DevoidEngine.Physics.PhysicsShapeType.Sphere
            //};
            

            lightGo = scene.AddGameObject("Light Object");
            LightComponent light = lightGo.AddComponent<LightComponent>();
            light.LightType = LightType.DirectionalLight;
            light.Intensity = 5;
            light.Radius = 100;
            light.InnerCutoff = 30;
            light.OuterCutoff = 35;
            light.Color = new Vector4(1, 1, 1, 1);
            lightGo.Transform.EulerAngles = new Vector3(45, 10, 0);
            lightGo.Transform.Position = new Vector3(5, 10, -7);
            AudioSource3D audio = lightGo.AddComponent<AudioSource3D>();

            AudioClip clip = Asset.Load<AudioClip>("tone.mp3")!;
            audio.Audio = clip;
            audio.Play();

            Engine.InputSystem.Map.Bind("Forward", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.W
            });

            Engine.InputSystem.Map.Bind("Backward", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.S
            });

            Engine.InputSystem.Map.Bind("Left", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.A
            });

            Engine.InputSystem.Map.Bind("Right", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.D
            });

            Engine.InputSystem.Map.Bind("LookX", new InputBinding()
            {
                DeviceType = InputDeviceType.Mouse,
                Control = (ushort)MouseAxis.DeltaX,
                IsClamped = false
            });

            Engine.InputSystem.Map.Bind("LookY", new InputBinding()
            {
                DeviceType = InputDeviceType.Mouse,
                Control = (ushort)MouseAxis.DeltaY,
                IsClamped = false
            });

            Engine.InputSystem.Map.Bind("Grab", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.G
            });

            Engine.InputSystem.Map.Bind("SpawnLight", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.L
            });

            Engine.InputSystem.Map.Bind("Impulse", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.P
            });

        }

        GameObject go = null!;
        GameObject meshGo = null!;
        GameObject meshGo1 = null!;
        GameObject lightGo = null!;

        Camera3D cam = null!;

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
            if (Engine.InputSystem.GetActionDown("Impulse"))
            {
                Console.WriteLine("Impulse Added");
                meshGo1.GetComponent<RigidBodyComponent>()!.AddForce(new Vector3(0, 750, 0));
            }

            if (Engine.InputSystem.GetActionDown("SpawnLight"))
            {
                GameObject lightGo = scene.AddGameObject("Light Object");
                LightComponent light = lightGo.AddComponent<LightComponent>();
                light.LightType = LightType.PointLight;
                light.Intensity = 10;
                light.Radius = 100;
                light.Color = new Vector4(1, 1, 1, 1);
                lightGo.Transform.EulerAngles = new Vector3(45, 0, 0);

                float posX = (float)rand.NextDouble() * 20;
                float posY = (float)rand.NextDouble() * 20;

                lightGo.Transform.Position = new Vector3(posX, 10, posY);
            }

            if (Engine.InputSystem.GetActionDown("Grab"))
            {
                if (Engine.Cursor.GetCursorState() == CursorState.Grabbed)
                {
                    Engine.Cursor.SetCursorState(CursorState.Normal);
                }
                else
                {
                    Engine.Cursor.SetCursorState(CursorState.Grabbed);
                }
            }

            if (Engine.Cursor.GetCursorState() != CursorState.Grabbed)
                return;

            var transform = go.Transform;

            float forward =
                Engine.InputSystem.GetAction("Forward") -
                Engine.InputSystem.GetAction("Backward");

            float right =
                Engine.InputSystem.GetAction("Left") -
                Engine.InputSystem.GetAction("Right");

            Vector3 moveDirection = Vector3.Zero;

            moveDirection += transform.Forward * forward;
            moveDirection += transform.Right * right;

            if (moveDirection.LengthSquared() > 0.0f)
            {
                moveDirection = Vector3.Normalize(moveDirection);
            }


            const float moveSpeed = 5.0f;

            transform.Position +=
                moveDirection *
                moveSpeed *
                deltaTime;

            float lookX = Engine.InputSystem.GetAction("LookX");
            float lookY = Engine.InputSystem.GetAction("LookY");

            const float sensitivity = 0.0025f;

            yaw -= lookX * sensitivity;
            pitch += lookY * sensitivity;

            pitch = Math.Clamp(
                pitch,
                -MathF.PI * 0.49f,
                 MathF.PI * 0.49f);

            Quaternion rotation =
                Quaternion.CreateFromYawPitchRoll(
                    yaw,
                    pitch,
                    0.0f);

            go.Transform.Rotation = rotation;


        }

        private float yaw;
        private float pitch;

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
