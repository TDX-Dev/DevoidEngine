using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Physics;
using DevoidEngine.Engine.Utilities;
using System;
using System.Numerics;

namespace DevoidEngine.Engine.Components
{
    public class FPSController : Component, ICollisionListener
    {
        public override string Type => nameof(FPSController);

        // ===============================
        // Inspector Fields (Unity-style)
        // ===============================

        public float MoveSpeed = 6f;
        public float Acceleration = 20f;
        public float AirControl = 0.3f;
        public float JumpForce = 10f;
        public float MouseSensitivity = 0.12f;
        public float MinPitch = -89f;
        public float MaxPitch = 89f;
        public float GroundCheckDistance = 5f;

        // ===============================
        // Shooting
        // ===============================

        public float FireRate = 0.05f;
        public float ProjectileSpeed = 40f;
        public float ProjectileMass = 0.2f;
        public Vector3 ProjectileScale = new Vector3(0.2f);

        private float fireTimer = 0f;
        private Mesh projectileMesh;

        // ===============================
        // Internal
        // ===============================

        private RigidBodyComponent rb;
        private Transform cameraPivot;

        private float yaw;
        private float pitch;

        private Vector2 moveInput;
        private Vector2 mouseDelta;
        private bool jumpRequested;

        // ===============================
        // Setup
        // ===============================

        public override void OnStart()
        {
            projectileMesh = new Mesh();
            projectileMesh.SetVertices(Primitives.GetCubeVertex());

            rb = gameObject.GetComponent<RigidBodyComponent>();
            if (rb == null)
                return;

            // Freeze physics rotation (we rotate manually)
            rb.FreezeRotationX = true;
            rb.FreezeRotationY = true;
            rb.FreezeRotationZ = true;

            // First child = camera pivot (Unity-style hierarchy)
            if (gameObject.children.Count > 0)
                cameraPivot = gameObject.children[0].transform;

            // Initialize yaw from current rotation
            yaw = MathHelper.RadToDeg(
                MathF.Atan2(
                    gameObject.transform.Forward.X,
                    gameObject.transform.Forward.Z
                )
            );
        }

        // ===============================
        // Variable Update (INPUT ONLY)
        // ===============================

        public override void OnUpdate(float dt)
        {
            if (rb == null) return;

            moveInput = Input.MoveAxis;
            mouseDelta += Input.MouseDelta;

            if (Input.JumpPressed)
            {
                jumpRequested = true;
            }

            fireTimer -= dt;

            if (Input.GetMouseDown(MouseButton.Left))
            {
                TryShoot();
            }
        }
        private void TryShoot()
        {
            if (fireTimer > 0f)
                return;

            fireTimer = FireRate;

            Vector3 spawnPosition = cameraPivot != null
                ? cameraPivot.Position
                : gameObject.transform.Position;

            Quaternion rotation = cameraPivot != null
                ? cameraPivot.Rotation
                : rb.Rotation;

            Vector3 forward =
                Vector3.Normalize(
                    Vector3.Transform(Vector3.UnitZ, rotation)
                );

            BulletComponent.Spawn(
                gameObject.Scene,
                projectileMesh,
                spawnPosition + forward * 2f,
                rotation,
                ProjectileScale,
                ProjectileSpeed,
                ProjectileMass
            );
        }

        // ===============================
        // Fixed Update (Physics + Rotation)
        // ===============================

        public override void OnFixedUpdate(float fixedDt)
        {
            if (rb == null) return;

            HandleRotation();
            HandleMovement(fixedDt);

            jumpRequested = false;
        }

        // ===============================
        // Mouse Look (Unity Style)
        // ===============================

        private void HandleRotation()
        {
            yaw += mouseDelta.X * MouseSensitivity;
            pitch -= mouseDelta.Y * MouseSensitivity;

            pitch = Math.Clamp(pitch, MinPitch, MaxPitch);

            // Rotate body on Y axis
            Quaternion bodyRotation =
                Quaternion.CreateFromAxisAngle(
                    -Vector3.UnitY,
                    MathHelper.DegToRad(yaw)
                );

            rb.Rotation = bodyRotation;

            // Rotate camera on X axis
            if (cameraPivot != null)
            {
                cameraPivot.LocalRotation =
                    Quaternion.CreateFromAxisAngle(
                        -Vector3.UnitX,
                        MathHelper.DegToRad(pitch)
                    );
            }

            mouseDelta = Vector2.Zero;
        }

        // ===============================
        // Movement (CharacterBody Style)
        // ===============================

        private void HandleMovement(float fixedDt)
        {
            Vector3 forward = Vector3.Transform(Vector3.UnitZ, rb.Rotation);
            Vector3 right = Vector3.Transform(-Vector3.UnitX, rb.Rotation);

            // Flatten to ground plane
            forward.Y = 0;
            right.Y = 0;

            forward = Vector3.Normalize(forward);
            right = Vector3.Normalize(right);

            Vector3 desiredMove =
                forward * moveInput.Y +
                right * moveInput.X;

            if (desiredMove.LengthSquared() > 1f)
                desiredMove = Vector3.Normalize(desiredMove);

            Vector3 currentVelocity = rb.LinearVelocity;

            Vector3 horizontalVelocity =
                new Vector3(currentVelocity.X, 0, currentVelocity.Z);

            Vector3 targetVelocity =
                desiredMove * MoveSpeed;

            bool grounded = IsGrounded();

            float control = grounded ? 1f : AirControl;

            horizontalVelocity = Vector3.Lerp(
                horizontalVelocity,
                targetVelocity,
                Acceleration * control * fixedDt
            );

            rb.LinearVelocity = new Vector3(
                horizontalVelocity.X,
                currentVelocity.Y,
                horizontalVelocity.Z
            );

            // Jump
            if (jumpRequested && grounded)
            {
                rb.LinearVelocity = new Vector3(
                    rb.LinearVelocity.X,
                    JumpForce,
                    rb.LinearVelocity.Z
                );
            }
        }

        // ===============================
        // Ground Check
        // ===============================

        private bool IsGrounded()
        {
            Vector3 origin = gameObject.transform.Position;
            Vector3 direction = -Vector3.UnitY;

            bool val = gameObject.Scene.Physics.Raycast(
                new Ray(origin, direction),
                GroundCheckDistance,
                out RaycastHit hit
            );

            return val;
        }

        public void OnCollisionEnter(GameObject other)
        {
            Console.WriteLine("Entered");
        }

        public void OnCollisionStay(GameObject other)
        {

        }

        public void OnCollisionExit(GameObject other)
        {
            Console.WriteLine("Left");
        }
    }
}