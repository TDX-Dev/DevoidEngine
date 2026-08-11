using DevoidEngine.AssetPipeline;
using DevoidEngine.Audio;
using DevoidEngine.Core;
using DevoidEngine.InputSystem;
using DevoidEngine.Physics;
using DevoidEngine.Util;
using OpenTK.Windowing.Common;
using System.Numerics;

namespace DevoidEngine.Components
{
    public class FirstPersonController : Component
    {
        public override string Type => nameof(FirstPersonController);

        // --------------------------------------------------------
        // Settings
        // --------------------------------------------------------

        public float WalkSpeed = 5f;
        public float SprintSpeed = 8f;

        public float JumpVelocity = 6f;

        public float MouseSensitivity = 0.0025f;

        public float CapsuleRadius = 0.5f;
        public float CapsuleHeight = 1.8f;

        public float EyeHeight = 1.3f;

        public bool GrabCursorOnStart = true;

        public bool AllowSprint = true;

        // --------------------------------------------------------

        private RigidBodyComponent rb = null!;
        private PickupComponent pickup = null!;

        private GameObject cameraPivot = null!;
        private GameObject cameraObject = null!;
        private GameObject audioObjectFootsteps = null!;

        private Camera3D camera = null!;
        private AudioSource3D audioSourceFootsteps = null!;

        private AudioClip[] footstepClips = null!;
        private AudioClip jumpLandClip = null!;

        private float footstepDistance;
        private int lastFootstep = -1;

        public float WalkStepDistance = 2.0f;
        public float SprintStepDistance = 1.8f;

        public float LandingVelocityThreshold = 2.5f;
        private float landingVelocity;

        private float yaw;
        private float pitch;

        // TODO: Replace with proper raycast
        private bool grounded = true;

        public override void OnAttach()
        {
            CreateRigidBody();
            CreateCameraHierarchy();
            CreateAudioObjects();
        }

        public override void OnStart()
        {
            rb = gameObject.GetComponent<RigidBodyComponent>()!;

            cameraPivot = gameObject.Scene!
                .GameObjects
                .Find(x => x.Name == $"{gameObject.Name}_CameraPivot")!;

            cameraObject = gameObject.Scene!
                .GameObjects
                .Find(x => x.Name == $"{gameObject.Name}_Camera")!;

            camera = cameraObject.GetComponent<Camera3D>()!;

            yaw = gameObject.Transform.EulerAngles.Y;
            pitch = 0;

            if (GrabCursorOnStart)
                Engine.Cursor.SetCursorState(CursorState.Grabbed);

            //GameObject go = gameObject.Scene.AddGameObject("Debug");
            //MeshRenderer mr = go.AddComponent<MeshRenderer>();
            //mr.Mesh = PrimitiveMeshes.GetCube();
            //go.SetParent(gameObject);
            //go.Transform.LocalPosition = new Vector3(0, -1, 0);
        }

        private void CreateRigidBody()
        {
            RigidBodyComponent? body =
                gameObject.GetComponent<RigidBodyComponent>();

            body ??= gameObject.AddComponent<RigidBodyComponent>();

            body.OverrideRotation = true;

            body.LockRotationX = true;
            body.LockRotationY = true;
            body.LockRotationZ = true;

            body.Shape = new PhysicsShapeDescription()
            {
                Type = PhysicsShapeType.Capsule,
                Radius = CapsuleRadius,
                Height = CapsuleHeight
            };
        }

        private void CreateAudioObjects()
        {
            audioObjectFootsteps = gameObject.Scene!.AddGameObject(
                $"{gameObject.Name}_audioObjectFootsteps");

            audioObjectFootsteps.SetParent(gameObject);
            audioObjectFootsteps.Transform.LocalPosition = new Vector3(0, -1, 0);

            audioSourceFootsteps = audioObjectFootsteps.AddComponent<AudioSource3D>();
            footstepClips =
            [
                Asset.Load<AudioClip>("Sounds/Step1.wav")!,
                Asset.Load<AudioClip>("Sounds/Step2.wav")!,
                Asset.Load<AudioClip>("Sounds/Step3.wav")!,
                Asset.Load<AudioClip>("Sounds/Step4.wav")!
            ];

            jumpLandClip = Asset.Load<AudioClip>("Sounds/Jump.wav")!;

        }

        private void CreateCameraHierarchy()
        {
            cameraPivot = gameObject.Scene!.AddGameObject(
                $"{gameObject.Name}_CameraPivot");

            cameraPivot.SetParent(gameObject);

            cameraPivot.Transform.LocalPosition =
                new Vector3(0, EyeHeight, 0);

            cameraObject = gameObject.Scene.AddGameObject(
                $"{gameObject.Name}_Camera");

            cameraObject.SetParent(cameraPivot);

            camera = cameraObject.AddComponent<Camera3D>();

            pickup = gameObject.AddComponent<PickupComponent>();
        }

        public override void OnUpdate(float dt)
        {
            if (Engine.Cursor.GetCursorState() != CursorState.Grabbed)
                return;

            pickup?.SetCamera( camera);

            UpdateGrounded();

            UpdateMouseLook(dt);
            UpdateMovement(dt);
            UpdateFootsteps(dt);
            UpdateLanding(); 
        }

        private void UpdateMouseLook(float dt)
        {
            float lookX = Engine.InputSystem.GetAction("LookX");
            float lookY = Engine.InputSystem.GetAction("LookY");

            yaw += lookX * MouseSensitivity;
            pitch += lookY * MouseSensitivity;

            pitch = Math.Clamp(
                pitch,
                -MathF.PI * 0.49f,
                 MathF.PI * 0.49f);

            //gameObject.Transform.Rotation =
            //    Quaternion.CreateFromAxisAngle(
            //        Vector3.UnitY,
            //        yaw);

            rb.Rotation = Quaternion.CreateFromAxisAngle(
                    Vector3.UnitY,
                    yaw);

            cameraPivot.Transform.LocalRotation =
                Quaternion.CreateFromAxisAngle(
                    Vector3.UnitX,
                    pitch);
        }

        private void UpdateMovement(float dt)
        {
            float forward =
                Engine.InputSystem.GetAction("Forward") -
                Engine.InputSystem.GetAction("Backward");

            float right =
                Engine.InputSystem.GetAction("Right") -
                Engine.InputSystem.GetAction("Left");

            Vector3 move = Vector3.Zero;

            move += gameObject.Transform.Forward * forward;
            move += gameObject.Transform.Right * right;

            move.Y = 0;

            if (move.LengthSquared() > 0)
                move = Vector3.Normalize(move);

            float speed = WalkSpeed;

            if (AllowSprint &&
                Engine.InputSystem.GetAction("Sprint") > 0.5f)
            {
                speed = SprintSpeed;
            }

            Vector3 velocity = rb.LinearVelocity;

            velocity.X = move.X * speed;
            velocity.Z = move.Z * speed;

            rb.LinearVelocity = velocity;

            // ----------------------------------------------------
            // TODO:
            // Replace this with a proper raycast or contact test.
            // grounded = ...
            // ----------------------------------------------------

            if (grounded &&
                Engine.InputSystem.GetActionDown("Jump"))
            {
                velocity = rb.LinearVelocity;
                velocity.Y = JumpVelocity;
                rb.LinearVelocity = velocity;
            }

            if (Engine.InputSystem.GetActionDown("Pickup"))
            {
                pickup?.TryPickup();
            }
        }
        private void UpdateFootsteps(float dt)
        {
            Vector2 horizontalVelocity = new(
                rb.LinearVelocity.X,
                rb.LinearVelocity.Z);

            float speed = horizontalVelocity.Length();

            if (!grounded ||
                rb.LinearVelocity.Y > 0.1f ||
                speed < 0.2f)
            {
                footstepDistance = 0;
                return;
            }

            footstepDistance += speed * dt;

            float stepDistance =
                (AllowSprint &&
                Engine.InputSystem.GetAction("Sprint") > 0.5f)
                    ? SprintStepDistance
                    : WalkStepDistance;

            if (footstepDistance >= stepDistance)
            {
                footstepDistance -= stepDistance;
                PlayRandomFootstep();
            }
        }

        private void PlayRandomFootstep()
        {
            int index;

            do
            {
                index = Random.Shared.Next(footstepClips.Length);
            }
            while (footstepClips.Length > 1 && index == lastFootstep);

            lastFootstep = index;

            audioSourceFootsteps.PlayOneShot(footstepClips[index], 0.05f);
        }

        private void UpdateLanding()
        {
            if (!grounded)
            {
                landingVelocity = Math.Min(landingVelocity, rb.LinearVelocity.Y);
            }
        }

        private void UpdateGrounded()
        {
            float skin = 0.05f;

            Vector3 origin = gameObject.Transform.Position +
                Vector3.UnitY * 0.1f;

            float rayLength =
                (CapsuleHeight * 0.6f) +
                CapsuleRadius +
                skin;

            Ray ray = new(origin, Vector3.UnitY * -1);

            bool isGrounded = gameObject.Scene!.Physics.Raycast(
                ray,
                rayLength,
                out RaycastHit hit);

            // Ignore hitting ourselves
            if (isGrounded && hit.HitObject == gameObject)
            {
                isGrounded = false;
            }

            SetGrounded(isGrounded);
        }

        public void SetGrounded(bool value)
        {
            if (!grounded && value)
            {
                if (landingVelocity <= -LandingVelocityThreshold)
                {
                    audioSourceFootsteps.PlayOneShot(jumpLandClip, 0.2f);
                }

                landingVelocity = 0;
                footstepDistance = 0;
            }

            grounded = value;
        }
        public Camera3D GetCamera()
        {
            return camera;
        }

        public GameObject GetCameraObject()
        {
            return cameraObject;
        }

        public GameObject GetCameraPivot()
        {
            return cameraPivot;
        }

        public void SetYaw(float value)
        {
            yaw = value;
        }

        public void SetPitch(float value)
        {
            pitch = Math.Clamp(
                value,
                -MathF.PI * 0.49f,
                 MathF.PI * 0.49f);
        }

        public float GetYaw()
        {
            return yaw;
        }

        public float GetPitch()
        {
            return pitch;
        }

    }
}