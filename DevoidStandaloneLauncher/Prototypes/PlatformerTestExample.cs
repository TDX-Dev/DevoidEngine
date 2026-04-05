using DevoidEngine.Engine.AssetPipeline;
using DevoidEngine.Engine.AudioSystem;
using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Physics;
using DevoidEngine.Engine.ProjectSystem;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Serialization;
using DevoidEngine.Engine.UI.Nodes;
using DevoidEngine.Engine.UI.Theme;
using DevoidEngine.Engine.UI.Theme.Styleboxes;
using DevoidStandaloneLauncher.Scripts;
using DevoidStandaloneLauncher.Utils;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidStandaloneLauncher.Prototypes
{
    public class PlatformerTestExample : Prototype
    {
        public override void OnInit()
        {
            DebugRenderSystem.AllowDebugDraw = true;
            DefaultInput.ConfigureInput();

            Scene scene = new Scene();
            loader.CurrentScene = scene;
            SceneManager.LoadScene(scene);

            scene.AddGameObject("Skybox").AddComponent<SkyboxComponent>();

            //GameObject player = scene.AddGameObject("Player Camera");
            //CameraComponent3D playerCamera = player.AddComponent<CameraComponent3D>();
            //player.AddComponent<FreeCameraComponent>();
            //playerCamera.isDefault = true;

            //RigidBodyComponent icospherePhysics = icosphereObject.AddComponent<RigidBodyComponent>();
            //icospherePhysics.Mass = 100;
            //icospherePhysics.Shape = new PhysicsShapeDescription()
            //{
            //    Type = PhysicsShapeType.Sphere,
            //    Radius = 1,
            //};

            Model demoModel = Asset.Load<Model>("cubey_boi/cubey_boi.gltf");
            GameObject demoObject = demoModel.Instantiate(scene);
            demoObject.Transform.Scale = Vector3.One;
            demoObject.Transform.Position = new Vector3(0, 5, 0);

            ThirdPersonController controller = demoObject.AddComponent<ThirdPersonController>();

            // Player physics
            RigidBodyComponent rigidBodyPlayer = demoObject.AddComponent<RigidBodyComponent>();
            rigidBodyPlayer.LockRotationX = true;
            rigidBodyPlayer.LockRotationY = false;
            rigidBodyPlayer.LockRotationZ = true;
            rigidBodyPlayer.Shape = new PhysicsShapeDescription()
            {
                Size = demoObject.Transform.Scale * 2,
                Type = PhysicsShapeType.Box
            };

            // Light
            GameObject originLight = scene.AddGameObject("Origin Light");
            LightComponent originLightComponent = originLight.AddComponent<LightComponent>();
            originLightComponent.Intensity = 100;
            originLight.SetParent(demoObject);
            originLight.Transform.LocalPosition = new Vector3(0, 0, 0);

            // ---------- CAMERA RIG ----------

            GameObject cameraYaw = scene.AddGameObject("CameraYaw");
            //cameraYaw.SetParent(demoObject, false);


            GameObject cameraPitch = scene.AddGameObject("CameraPitch");
            cameraPitch.SetParent(cameraYaw, false);

            GameObject cameraObject = scene.AddGameObject("Camera");
            var camera = cameraObject.AddComponent<CameraComponent3D>();
            cameraObject.SetParent(cameraPitch);

            cameraObject.Transform.LocalPosition = new Vector3(0, 0, -10);
            camera.IsDefault = true;

            controller.Camera = cameraYaw.Transform;
            controller.CameraPitch = cameraPitch.Transform;

            // Platforms
            Model platformModel = Asset.Load<Model>("platform/platform.gltf");
            GameObject platformObject = platformModel.Instantiate(scene);

            SetPhysicsForColliderPlatforms(platformObject);

            //StaticColliderComponent platformPhysics = platformObject.AddComponent<StaticColliderComponent>();

            //platformPhysics.Shape = new PhysicsShapeDescription()
            //{
            //    Size = new Vector3(10, 0.2f, 10) * 2,
            //    Type = PhysicsShapeType.Box,
            //};
            //platformPhysics.Material = new PhysicsMaterial()
            //{
            //    Friction = 1,
            //    Restitution = 0,
            //};


            scene.Play();

        }

        void SetPhysicsForColliderPlatforms(GameObject gameObject)
        {
            Console.WriteLine(gameObject.Name);
            if (IsCollideable(gameObject.Name))
            {
                var meshRenderer = gameObject.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    var collider = gameObject.AddComponent<StaticColliderComponent>();
                    collider.Shape = new PhysicsShapeDescription()
                    {
                        Size = gameObject.Transform.Scale * 2,
                        Type = PhysicsShapeType.Box
                    };

                }
            }

            foreach (var child in gameObject.children)
            {
                SetPhysicsForColliderPlatforms(child);
            }
        }

        bool IsCollideable(string name)
        {
            return name == "Collideable" || name.StartsWith("Collideable.");
        }

    }
}
