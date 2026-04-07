using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.InputSystem;
using DevoidEngine.Engine.Physics;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Utilities;
using SoLoud;
using System;
using System.Numerics;

namespace DevoidStandaloneLauncher.Scripts
{

    public class ThirdPersonController : Component
    {
        public override string Type => nameof(ThirdPersonController);

        // Movement
        public float MoveSpeed = 10f;
        public float Acceleration = 20f;
        public float AirControl = 0.2f;

        // Jump
        public float JumpForce = 6f;

        // Rotation
        public float RotationSpeed = 12f;

        // Camera
        public Transform3D Camera;
        public Transform3D CameraPitch;
        public Transform3D CameraFollowTarget;

        private RigidBodyComponent body;

        private Vector2 movementInput;
        private bool jumpPressed;

        public float MouseSensitivity = 0.15f;
        public float PitchLimit = 80f;
        float cameraYaw;
        float cameraPitch;

        int jumpsUsed = 0;
        public int MaxJumps = 2;
        bool wasGrounded = false;

        public int OrbsCollected = 0;

        public override void OnStart()
        {
            body = gameObject.GetComponent<RigidBodyComponent>();
        }

        public override void OnUpdate(float dt)
        {
            if (body == null)
            {
                body = gameObject.GetComponent<RigidBodyComponent>();
                return;
            }

            if (gameObject.Transform.Position.Y < -20)
            {

                body.Position = new(0, 5, 0);
            }


            if (Camera != null)
            {
                Vector3 target = gameObject.Transform.Position + new Vector3(0, 2f, 0);

                Camera.Position = Vector3.Lerp(Camera.Position, target, 5f * dt);
            }

            CursorState state = Cursor.GetCursorState();

            if (Input.GetActionDown("Capture"))
            {

                if (state == CursorState.Normal)
                {
                    Cursor.SetCursorState(CursorState.Grabbed);
                } else
                {
                    Cursor.SetCursorState(CursorState.Normal);
                }
            }
            if (state == CursorState.Normal)
                return;


            movementInput = GetMovementInput();

            if (Input.GetActionDown("Up"))
                jumpPressed = true;

            UpdateCameraOrbit();
        }

        void UpdateCameraOrbit()
        {
            if (Camera == null || CameraPitch == null)
                return;

            float mouseX = Input.GetAction("LookX");
            float mouseY = Input.GetAction("LookY");

            cameraYaw -= mouseX * MouseSensitivity;
            cameraPitch += mouseY * MouseSensitivity;

            cameraPitch = Math.Clamp(cameraPitch, -PitchLimit, PitchLimit);

            float yawRad = cameraYaw * (MathF.PI / 180f);
            float pitchRad = cameraPitch * (MathF.PI / 180f);

            // Rotate camera rig
            Camera.LocalRotation =
                Quaternion.CreateFromAxisAngle(Vector3.UnitY, yawRad);

            CameraPitch.LocalRotation =
                Quaternion.CreateFromAxisAngle(Vector3.UnitX, pitchRad);

        }

        void RotateTowards(Vector3 direction, float dt)
        {
            if (direction.LengthSquared() < 0.0001f)
                return;

            direction.Y = 0;
            direction = Vector3.Normalize(direction);

            float targetAngle = MathF.Atan2(direction.X, direction.Z);

            float modelForwardOffset = MathF.PI;

            Quaternion targetRotation =
                Quaternion.CreateFromAxisAngle(Vector3.UnitY, targetAngle + modelForwardOffset);

            body.Rotation =
                Quaternion.Slerp(body.Rotation, targetRotation, RotationSpeed * dt);

            body.AngularVelocity = Vector3.Zero;
        }

        public override void OnFixedUpdate(float dt)
        {
            if (body == null)
            {
                body = gameObject.GetComponent<RigidBodyComponent>();
                return;
            }
            bool grounded = IsGrounded();

            // Detect landing
            if (grounded && !wasGrounded)
            {
                jumpsUsed = 0;
            }

            wasGrounded = grounded;

            HandleMovement(dt);
            HandleJump();
        }

        Vector2 GetMovementInput()
        {
            float x = Input.GetAction("Left") - Input.GetAction("Right");
            float y = Input.GetAction("Forward") - Input.GetAction("Backward");


            Vector2 input = new Vector2(x, y);

            if (input.LengthSquared() > 1)
                input = Vector2.Normalize(input);

            return input;
        }

        void HandleMovement(float dt)
        {




            float yawRad = cameraYaw * (MathF.PI / 180f);

            Vector3 camForward = new Vector3(
                MathF.Sin(yawRad),
                0,
                MathF.Cos(yawRad)
            );

            Vector3 camRight = new Vector3(
                camForward.Z,
                0,
                -camForward.X
            );

            Vector3 moveDir =
                camForward * movementInput.Y +
                camRight * movementInput.X;

            if (moveDir.LengthSquared() > 1)
                moveDir = Vector3.Normalize(moveDir);

            bool grounded = IsGrounded();
            float control = grounded ? 1f : AirControl;

            Vector3 velocity = body.LinearVelocity;
            Vector3 targetVelocity = moveDir * MoveSpeed;

            velocity.X = MathHelper.Lerp(
                velocity.X,
                targetVelocity.X,
                Acceleration * control * dt
            );

            velocity.Z = MathHelper.Lerp(
                velocity.Z,
                targetVelocity.Z,
                Acceleration * control * dt
            );

            //      if is_on_floor():
            //          if direction:
            //              velocity.x = direction.x * SPEED
            //              velocity.z = direction.z * SPEED
            //      else:
            //              velocity.x = lerp(velocity.x, direction.x * SPEED, delta * 2)
            //              velocity.z = lerp(velocity.z, direction.z * SPEED, delta * 2)

            body.LinearVelocity = velocity;

            if (moveDir.LengthSquared() > 0.001f)
                RotateTowards(moveDir, dt);
        }

        void HandleJump()
        {
            if (!jumpPressed)
                return;

            jumpPressed = false;

            if (jumpsUsed >= MaxJumps)
                return;

            Vector3 vel = body.LinearVelocity;

            // reset downward velocity so jumps feel responsive
            if (vel.Y < 0)
                vel.Y = 0;

            vel.Y += JumpForce;

            body.LinearVelocity = vel;

            jumpsUsed++;
        }

        private bool IsGrounded()
        {
            Vector3 origin = gameObject.Transform.Position;

            return gameObject.Scene.Physics.Raycast(
                new Ray(origin - Vector3.UnitY, -Vector3.UnitY),
                0.2f,
                out RaycastHit hit
            );
        }
    }
}
