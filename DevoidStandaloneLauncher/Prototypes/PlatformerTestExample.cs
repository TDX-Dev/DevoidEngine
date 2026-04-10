using DevoidEngine.Engine.AssetPipeline;
using DevoidEngine.Engine.Assets;
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

        Scene scene;
        GameObject spotLight;
        public override void OnInit()
        {
            DebugRenderSystem.AllowDebugDraw = false;
            DefaultInput.ConfigureInput();

            scene = new Scene();
            loader.CurrentScene = scene;
            SceneManager.LoadScene(scene);
            scene.AddGameObject("Skybox").AddComponent<SkyboxComponent>();


            spotLight = scene.AddGameObject("Origin Light");
            LightComponent spotLightComponent = spotLight.AddComponent<LightComponent>();
            spotLightComponent.Intensity = 50;
            spotLightComponent.Radius = 100;
            spotLightComponent.LightType = LightType.SpotLight;
            spotLightComponent.CastShadows = true;
            spotLightComponent.InnerCutoff = 15;
            spotLightComponent.OuterCutoff = 20;
            spotLight.Transform.LocalPosition = new Vector3(0, 10, -5);
            spotLight.Transform.EulerAngles = new Vector3(45, 0, 0);


            GameObject spotLight1 = scene.AddGameObject("Origin Light");
            LightComponent spotLightComponent1 = spotLight1.AddComponent<LightComponent>();
            spotLightComponent1.Intensity = 50;
            spotLightComponent1.Radius = 20;
            spotLightComponent1.LightType = LightType.SpotLight;
            spotLightComponent1.CastShadows = true;
            spotLightComponent1.InnerCutoff = 6;
            spotLightComponent1.OuterCutoff = 10;
            spotLight1.Transform.LocalPosition = new Vector3(2, 4, -7);
            spotLight1.Transform.EulerAngles = new Vector3(20, 45, 0);


            //Model demoModel = Asset.Load<Model>("goblin_j/practice.gltf");
            Model demoModel = Asset.Load<Model>("cubey_boi/cubey_boi.gltf");
            GameObject demoObject = demoModel.Instantiate(scene);
            demoObject.Name = "Player";
            demoObject.Transform.Scale = Vector3.One;
            demoObject.Transform.Position = new Vector3(0, 5, 0);

            controller = demoObject.AddComponent<ThirdPersonController>();

            // Player physics
            RigidBodyComponent rigidBodyPlayer = demoObject.AddComponent<RigidBodyComponent>();
            rigidBodyPlayer.LockRotationX = true;
            rigidBodyPlayer.LockRotationY = false;
            rigidBodyPlayer.LockRotationZ = true;
            rigidBodyPlayer.Mass = 20;
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


            GameObject canvasObject = scene.AddGameObject("canvas");
            CanvasComponent canvasComp = canvasObject.AddComponent<CanvasComponent>();
            canvasComp.Canvas.Align = AlignItems.Start;
            canvasComp.Canvas.Justify = JustifyContent.Start;

            ContainerNode UIContainer = new ContainerNode()
            {

            };

            FlexboxNode orbInfoContainer = new FlexboxNode()
            {
                Align = AlignItems.Center,
                Justify = JustifyContent.Start,
                Padding = Padding.GetAll(10),
                Gap = 10
            };

            orbLabel = new LabelNode("Collected: 0", 16);
            ContainerNode orbTexture = new ContainerNode() { Size = new Vector2(50, 50) };
            orbTexture.AddStyleBoxOverride(StyleKeys.Normal, new StyleBoxTexture()
            {
                Texture = Asset.Load<Texture2D>("orb/orb.png")
            });

            orbInfoContainer.Add(orbTexture);
            orbInfoContainer.Add(orbLabel);
            UIContainer.Add(orbInfoContainer);

            canvasComp.Canvas.Add(UIContainer);

            scene.Play();

            //File.WriteAllBytes(ProjectManager.Current.AssetPath + "\\mainscene.scene", MessagePackSerializer.Serialize(SceneSerializer.Serialize(scene)));
        }
        LabelNode orbLabel;
        ThirdPersonController controller;

        private float time = 0f;

        public override void OnFixedUpdate(float delta)
        {
            time += delta;

            float bob = MathF.Sin(time) * 3f; // amplitude = 3 units

            spotLight.Transform.Position = new Vector3(
                0,
                10 + bob,
                -5
            );

            orbLabel.Text = $"Collected: {controller.OrbsCollected}";
        }


        void SetPhysicsForColliderPlatforms(GameObject gameObject)
        {
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


                    Model orbModel = Asset.Load<Model>("orb/orb.gltf");
                    GameObject orbObject = orbModel.Instantiate(scene);
                    orbObject.Transform.Position = gameObject.Transform.Position + new Vector3(0, 2, 0);

                    AreaComponent areaCollider = orbObject.AddComponent<AreaComponent>();
                    areaCollider.Shape = new PhysicsShapeDescription()
                    {
                        Type = PhysicsShapeType.Box,
                        Size = orbObject.Transform.Scale * 2,
                    };

                    orbObject.AddComponent<OrbComponent>();

                }
            } else if (IsMovingCollider(gameObject.Name, out int type))
            {
                var collider = gameObject.AddComponent<RigidBodyComponent>();
                
                collider.StartKinematic = true;
                collider.Shape = new PhysicsShapeDescription()
                {
                    Size = gameObject.Transform.Scale * 2,
                    Type = PhysicsShapeType.Box
                };
                collider.Material = new PhysicsMaterial()
                {
                    AngularDamping = 1,
                    LinearDamping = 1,
                    Friction = 1,
                    Restitution = 0,
                };
                var movCollider = gameObject.AddComponent<MovingCollider>();

                movCollider.MoveSpeed = movColliderType[type];

            } else if (IsRigidbody(gameObject.Name))
            {
                var collider = gameObject.AddComponent<RigidBodyComponent>();
                collider.Shape = new PhysicsShapeDescription()
                {
                    Size = gameObject.Transform.Scale * 2,
                    Type = PhysicsShapeType.Box
                };
            }

            foreach (var child in gameObject.children)
            {
                SetPhysicsForColliderPlatforms(child);
            }
        }

        Dictionary<int, int> movColliderType = new Dictionary<int, int>()
        {
            { 1, 2 },
            { 2, 1 },
            { 3, 4 },
        };

        bool IsRigidbody(string name)
        {
            return name == "Collideable_Rigidbody" || name.StartsWith("Collideable_Rigidbody.");
        }

        bool IsCollideable(string name)
        {
            return name == "Collideable" || name.StartsWith("Collideable.");
        }

        bool IsMovingCollider(string name, out int type)
        {
            type = -1;

            if (!name.StartsWith("Collideable_MP"))
                return false;

            // Remove Blender duplicate suffix
            int dotIndex = name.IndexOf('.');
            if (dotIndex != -1)
                name = name.Substring(0, dotIndex);

            // Find _T
            int typeIndex = name.IndexOf("_T");
            if (typeIndex == -1)
                return true; // valid moving collider but no type

            string typeStr = name.Substring(typeIndex + 2);

            if (int.TryParse(typeStr, out type))
                return true;

            return true;
        }

    }
}
