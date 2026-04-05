using DevoidEngine.Engine.AssetPipeline;
using DevoidEngine.Engine.AudioSystem;
using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Physics;
using DevoidEngine.Engine.ProjectSystem;
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
            DefaultInput.ConfigureInput();

            Scene scene = new Scene();
            loader.CurrentScene = scene;
            SceneManager.LoadScene(scene);

            scene.AddGameObject("Skybox").AddComponent<SkyboxComponent>();

            GameObject player = scene.AddGameObject("Player Camera");
            CameraComponent3D playerCamera = player.AddComponent<CameraComponent3D>();
            player.AddComponent<FreeCameraComponent>();
            playerCamera.isDefault = true;

            GameObject originLight = scene.AddGameObject("Origin Light");
            LightComponent originLightComponent = originLight.AddComponent<LightComponent>();
            originLightComponent.Intensity = 100;
            originLight.Transform.Position = new Vector3(0, 5, 0);


            Model model = Asset.Load<Model>("icosphere.gltf");
            GameObject icosphereObject = model.Instantiate(scene);

            icosphereObject.Transform.Position = new Vector3(0, 0, 5);

            RigidBodyComponent icospherePhysics = icosphereObject.AddComponent<RigidBodyComponent>();
            icospherePhysics.Shape = new PhysicsShapeDescription()
            {
                Type = PhysicsShapeType.Sphere,
                Radius = 1,
            };


            scene.Play();

        }

    }
}
