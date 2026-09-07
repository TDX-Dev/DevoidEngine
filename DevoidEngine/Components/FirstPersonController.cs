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

        // ============================================================
        // General Movement
        // ============================================================

        public float WalkSpeed = 15f;
        public float SprintSpeed = 20f;

        public float Gravity = 20f;
        public float JumpVelocity = 12f;

        public float MouseSensitivity = 0.00025f;

        // ============================================================
        // Camera
        // ============================================================

        public float CapsuleRadius = 0.5f;
        public float CapsuleHeight = 1.8f;

        public float EyeHeight = 1.3f;

        public bool GrabCursorOnStart = true;
        public bool AllowSprint = true;

        // ============================================================
        // Ground Movement
        // ============================================================

        public float FloorAcceleration = 7f;
        public float FloorDrag = 8f;

        // ============================================================
        // Air Movement
        // ============================================================

        public float AirAcceleration = 0.5f;
        public float AirSpeed = 16f;
        public float AirDrag = 0.1f;

        // ============================================================
        // Air Strafing
        // ============================================================

        public float AirStrafeModifier = 1f;

        public float MinStrafeAngle = 0f;
        public float MaxStrafeAngle = 180f;

        // ============================================================
        // Coyote Time
        // ============================================================

        public float CoyoteTime = 0.2f;

        private float coyoteTimer;
        private bool canJump = true;
        //private bool hasJumped = false;
        private bool jumpQueued = false;

        // ============================================================
        // Sliding
        // ============================================================

        public float CrouchSpeed = 8f;
        public float CrouchAcceleration = 4f;

        public float SlideAcceleration = 0.8f;

        public float SlideDrag = 0.6f;

        public float StartSlideSpeed = 13f;
        public float EndSlideSpeed = 11f;

        public float SlideBoostForce = 4f;
        public float SlideBoostTime = 2f;

        public float MaxSlideSlopeSpeed = 25f;
        public float SlideSlopeForce = 4f;

        private bool canSlideBoost = true;

        private float slideCurvePoint = 0f;

        public float SlideDragTime = 0.6f;

        // ============================================================
        // Wall Running
        // ============================================================

        public float WallRunHeight = 4f;
        public float WallRunTime = 2f;
        public float WallRunResetTime = 1f;

        public float WallRunJumpForce = 12f;

        public float WallRunTilt = 15f;
        public float WallRunTiltSpeed = 8f;

        private float currentCameraTilt = 0f;

        private Vector3 wallRunStartVelocity;

        private float wallRunPoint;

        private bool hasLeftWallRun;
        private bool hasRightWallRun;

        private bool leftWallRun = true;

        private Vector3 previousWallNormal = Vector3.UnitY;
        private Vector3 previousWallRunPoint =
            new(float.NegativeInfinity);

        // ============================================================
        // Collision / Ground Detection
        // ============================================================

        public float GroundRayLength = 1.2f;

        public float GroundNormalThreshold = 0.6f;

        public float WallDetectionDistance = 0.7f;

        // ============================================================
        // Landing
        // ============================================================

        public float LandingVelocityThreshold = 2.5f;

        // ============================================================
        // State
        // ============================================================

        private MovementState currentState = MovementState.Air;
        private MovementState previousState = MovementState.Air;

        private enum MovementState
        {
            Ground,
            Air,
            Sliding,
            WallRunning
        }

        // ============================================================
        // Physics
        // ============================================================

        private RigidBodyComponent rb = null!;
        private PickupComponent pickup = null!;

        // ============================================================
        // Camera
        // ============================================================

        private GameObject cameraPivot = null!;
        private GameObject cameraObject = null!;

        private Camera3D camera = null!;

        private float yaw;
        private float pitch;

        public override void OnStart()
        {
            CreateRigidBody();
            CreateCameraHierarchy();

            rb = gameObject.GetComponent<RigidBodyComponent>()!;

            cameraPivot = gameObject.Scene!
                .GameObjects
                .Find(x => x.Name == $"{gameObject.Name}_CameraPivot")!;

            cameraObject = gameObject.Scene!
                .GameObjects
                .Find(x => x.Name == $"{gameObject.Name}_Camera")!;

            camera = cameraObject.GetComponent<Camera3D>()!;

            yaw = gameObject.Transform.EulerAngles.Y;
            pitch = 0f;

            pickup.SetCamera(camera);
            pickup.SetHoldPoint(holdPoint);
            pickup.SetController(this);

            if (GrabCursorOnStart)
                Engine.Cursor.SetCursorState(CursorState.Grabbed);
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

        private GameObject holdPoint = null!;

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

            // ============================================================
            // Hold Point
            // ============================================================

            holdPoint = gameObject.Scene.AddGameObject(
                $"{gameObject.Name}_HoldPoint");

            holdPoint.SetParent(gameObject);

            // Adjust these to position the gun in front of the camera.
            holdPoint.Transform.LocalPosition = new Vector3(1.3f, 1.3f, 1.6f);

            // ============================================================
            // Pickup
            // ============================================================

            pickup = gameObject.AddComponent<PickupComponent>();
        }

        // ============================================================
        // Render / Input Update
        // ============================================================

        public override void OnUpdate(float dt)
        {
            if (Engine.Cursor.GetCursorState() != CursorState.Grabbed)
                return;


            UpdateMouseLook(dt);

            if (Engine.InputSystem.GetActionDown("Jump"))
            {
                jumpQueued = true;
            }

        }

        // ============================================================
        // Physics Update
        // ============================================================

        public override void OnFixedUpdate(float dt)
        {
            UpdateGrounded(dt);

            UpdateCoyoteTime(dt);

            switch (currentState)
            {
                case MovementState.Ground:
                    Ground(dt);
                    break;

                case MovementState.Air:
                    Air(dt);
                    break;

                case MovementState.Sliding:
                    Slide(dt);
                    break;

                case MovementState.WallRunning:
                    WallRun(dt);
                    break;
            }

            HandleJump();
        }

        // ============================================================
        // Mouse Look
        // ============================================================

        private void UpdateMouseLook(float dt)
        {
            float lookX =
                Engine.InputSystem.GetAction("LookX");

            float lookY =
                Engine.InputSystem.GetAction("LookY");

            yaw += lookX * MouseSensitivity * 0.01f;
            pitch += lookY * MouseSensitivity * 0.01f;

            pitch = Math.Clamp(
                pitch,
                -MathF.PI * 0.49f,
                MathF.PI * 0.49f);

            rb.Rotation =
                Quaternion.CreateFromAxisAngle(
                    Vector3.UnitY,
                    yaw);

            float targetTilt = 0f;

            if (currentState == MovementState.WallRunning)
            {
                targetTilt =
                    leftWallRun
                        ? WallRunTilt
                        : -WallRunTilt;
            }

            currentCameraTilt = MathHelper.Lerp(currentCameraTilt, targetTilt, Math.Clamp(WallRunTiltSpeed * dt, 0f, 1f));

            Quaternion pitchRotation =
                Quaternion.CreateFromAxisAngle(
                    Vector3.UnitX,
                    pitch);

            Quaternion rollRotation =
                Quaternion.CreateFromAxisAngle(
                    Vector3.UnitZ,
                    currentCameraTilt *
                    MathF.PI / 180f);

            cameraPivot.Transform.LocalRotation =
                Quaternion.Normalize(
                    pitchRotation *
                    rollRotation);
        }

        // ============================================================
        // Ground
        // ============================================================

        private void Ground(float dt)
        {
            Vector3 velocity = rb.LinearVelocity;

            velocity.Y = 0f;

            rb.LinearVelocity = velocity;

            bool crouching =
                Engine.InputSystem.GetAction("Crouch") > 0.5f;

            float speed =
                crouching
                    ? CrouchSpeed
                    : GetMovementSpeed();

            float acceleration =
                crouching
                    ? CrouchAcceleration
                    : FloorAcceleration;

            Move(
                dt,
                acceleration,
                FloorDrag,
                speed);

            if (crouching &&
                HorizontalSpeed() > StartSlideSpeed)
            {
                ToSlide();
                return;
            }

            GroundToAir();
        }

        // ============================================================
        // Air
        // ============================================================

        private void Air(float dt)
        {
            Move(
                dt,
                AirAcceleration,
                AirDrag,
                AirSpeed);

            if (IsGrounded())
            {
                canJump = true;
                coyoteTimer = CoyoteTime;

                bool crouching =
                    Engine.InputSystem.GetAction("Crouch") > 0.5f;

                if (crouching &&
                    HorizontalSpeed() > StartSlideSpeed)
                {
                    ToSlide();
                }
                else
                {
                    ChangeState(MovementState.Ground);

                    hasLeftWallRun = false;
                    hasRightWallRun = false;
                }

                return;
            }

            if (jumpQueued)
                coyoteTimer = 0f;

            if (TryGetWallNormal(
                    GetMovementDirection(),
                    out Vector3 wallNormal))
            {
                if (wallRunPoint < 1f)
                {
                    bool isLeft =
                        IsWallRunningLeft(wallNormal);

                    if (!((isLeft && hasLeftWallRun) ||
                          (!isLeft && hasRightWallRun)))
                    {
                        wallRunPoint = 0f;

                        if (isLeft)
                        {
                            hasLeftWallRun = true;
                            hasRightWallRun = false;
                        }
                        else
                        {
                            hasLeftWallRun = false;
                            hasRightWallRun = true;
                        }

                        leftWallRun = isLeft;
                    }

                    ChangeState(MovementState.WallRunning);

                    wallRunStartVelocity =
                        rb.LinearVelocity;
                }
            }
        }

        // ============================================================
        // Sliding
        // ============================================================

        private void Slide(float dt)
        {
            bool crouching =
                Engine.InputSystem.GetAction("Crouch") > 0.5f;

            if (!crouching)
            {
                ChangeState(MovementState.Ground);
                slideCurvePoint = 0f;
                return;
            }

            slideCurvePoint +=
                dt / SlideDragTime;

            slideCurvePoint =
                Math.Clamp(slideCurvePoint, 0f, 1f);

            Vector3 velocity =
                rb.LinearVelocity;

            if (IsGrounded() &&
                TryGetGroundNormal(out Vector3 floorNormal))
            {
                bool movingDownSlope =
                    Vector3.Dot(
                        velocity,
                        floorNormal) > 0f;

                if (movingDownSlope &&
                    velocity.Length() < MaxSlideSlopeSpeed)
                {
                    ApplyForce(
                        velocity.LengthSquared() > 0.001f
                            ? Vector3.Normalize(velocity) *
                              dt *
                              SlideSlopeForce
                            : Vector3.Zero);
                }
            }

            Move(
                dt,
                SlideAcceleration,
                SlideDrag,
                GetMovementSpeed());

            if (HorizontalSpeed() < EndSlideSpeed)
            {
                ChangeState(MovementState.Ground);

                slideCurvePoint = 0f;
            }

            if (!IsGrounded())
            {
                ChangeState(MovementState.Air);

                slideCurvePoint = 0f;
            }
        }

        // ============================================================
        // Wall Run
        // ============================================================

        private void WallRun(float dt)
        {
            if (!TryGetWallNormal(
                    GetMovementDirection(),
                    out Vector3 wallNormal))
            {
                ChangeState(MovementState.Air);
                ResetWallRun();

                return;
            }

            Vector3 leftWallDirection =
                Vector3.Normalize(
                    Vector3.Cross(
                        Vector3.UnitY,
                        wallNormal));

            Vector3 rightWallDirection =
                -leftWallDirection;

            Vector3 newDirection =
                Vector3.Dot(
                    leftWallDirection,
                    wallRunStartVelocity)
                    >
                    Vector3.Dot(
                        rightWallDirection,
                        wallRunStartVelocity)
                    ? leftWallDirection
                    : rightWallDirection;

            float speed =
                Math.Clamp(
                    wallRunStartVelocity.Length(),
                    GetMovementSpeed() / 2f,
                    GetMovementSpeed() * 2f);

            Vector3 velocity =
                newDirection * speed;

            // Push slightly away from the wall.
            velocity -= wallNormal * 2f;

            // Add upward wall-running force.
            float wallRunProgress =
                Math.Clamp(
                    wallRunPoint,
                    0f,
                    1f);

            velocity +=
                Vector3.UnitY *
                wallRunProgress *
                WallRunHeight;

            rb.LinearVelocity = velocity;

            wallRunPoint +=
                dt / WallRunTime;

            if (wallRunPoint >= 1f)
            {
                ChangeState(MovementState.Air);
                ResetWallRun();

                return;
            }

            if (Engine.InputSystem.GetActionDown("Jump"))
            {
                previousWallRunPoint =
                    gameObject.Transform.Position;

                Vector3 jumpDirection =
                    Vector3.Normalize(
                        Vector3.UnitY +
                        wallNormal / 2f);

                ApplyForce(
                    jumpDirection *
                    WallRunJumpForce);

                ChangeState(MovementState.Air);

                ResetWallRun();
            }

            previousWallNormal =
                wallNormal;
        }

        // ============================================================
        // Main Movement Calculation
        // ============================================================

        private void Move(
            float dt,
            float acceleration,
            float drag,
            float speed)
        {
            Vector3 direction =
                GetMovementDirection();

            Vector3 wishVelocity =
                direction * speed;

            // --------------------------------------------------------
            // Air Strafing
            // --------------------------------------------------------

            if (currentState ==
                MovementState.Air &&
                direction.LengthSquared() > 0.0001f)
            {
                float angle =
                    GetHorizontalAngle(
                        rb.LinearVelocity,
                        wishVelocity);

                float samplePoint =
                    (angle - MinStrafeAngle) /
                    (MaxStrafeAngle - MinStrafeAngle);

                samplePoint =
                    Math.Clamp(
                        samplePoint,
                        0f,
                        1f);

                // Equivalent to:
                //
                // wish_vel *=
                //     1.0 +
                //     airStrafeCurve.sample(samplePoint)
                //     * airStrafeModifier
                //
                // Since we don't have Godot's Curve,
                // use a simple smooth curve.

                float strafeCurve =
                    AirStrafeCurve(samplePoint);

                wishVelocity *=
                    1f +
                    strafeCurve *
                    AirStrafeModifier;
            }

            // --------------------------------------------------------
            // Accelerate / Decelerate
            // --------------------------------------------------------

            Vector3 velocity =
                rb.LinearVelocity;

            if (direction.LengthSquared() > 0.0001f)
            {
                if (currentState ==
                    MovementState.Sliding)
                {
                    float newVelocityLength =
                        Vector3.Lerp(
                            velocity,
                            Vector3.Zero,
                            drag * dt).Length();

                    Vector3 currentDirection =
                        velocity.LengthSquared() >
                        0.0001f
                            ? Vector3.Normalize(velocity)
                            : direction;

                    Vector3 newDirection =
                        Vector3.Lerp(
                            currentDirection,
                            Vector3.Normalize(wishVelocity),
                            acceleration * dt);

                    if (newDirection.LengthSquared() >
                        0.0001f)
                    {
                        newDirection =
                            Vector3.Normalize(newDirection);
                    }

                    velocity =
                        newDirection *
                        newVelocityLength;
                }
                else
                {
                    velocity =
                        Vector3.Lerp(
                            velocity,
                            wishVelocity,
                            acceleration * dt);
                }
            }
            else
            {
                velocity =
                    Vector3.Lerp(
                        velocity,
                        wishVelocity,
                        drag * dt);
            }

            // --------------------------------------------------------
            // Gravity
            // --------------------------------------------------------

            if (currentState ==
                MovementState.Air)
            {
                velocity.Y -=
                    Gravity * dt;
            }

            // --------------------------------------------------------
            // Wall Sliding
            // --------------------------------------------------------

            if (currentState !=
                MovementState.WallRunning)
            {
                if (TryGetWallNormal(
                        direction,
                        out Vector3 wallNormal))
                {
                    Vector3 horizontal =
                        new(
                            velocity.X,
                            0f,
                            velocity.Z);

                    Vector3 horizontalWallNormal =
                        new(
                            wallNormal.X,
                            0f,
                            wallNormal.Z);

                    if (horizontalWallNormal.LengthSquared() >
                        0.0001f)
                    {
                        horizontalWallNormal =
                            Vector3.Normalize(
                                horizontalWallNormal);

                        // Remove only the velocity pointing
                        // INTO the wall.
                        float intoWall =
                            Vector3.Dot(
                                horizontal,
                                horizontalWallNormal);

                        if (intoWall < 0f)
                        {
                            horizontal -=
                                intoWall *
                                horizontalWallNormal;
                        }

                        velocity.X =
                            horizontal.X;

                        velocity.Z =
                            horizontal.Z;
                    }
                }
            }

            rb.LinearVelocity =
                velocity;
        }

        // ============================================================
        // Movement Direction
        // ============================================================

        private Vector3 GetMovementDirection()
        {
            float forward =
                Engine.InputSystem.GetAction("Backward") -
                Engine.InputSystem.GetAction("Forward");

            float right =
                Engine.InputSystem.GetAction("Right") -
                Engine.InputSystem.GetAction("Left");

            Vector3 forwardDirection =
                Vector3.Transform(
                    -Vector3.UnitZ,
                    Quaternion.CreateFromAxisAngle(
                        Vector3.UnitY,
                        yaw));

            Vector3 rightDirection =
                Vector3.Transform(
                    Vector3.UnitX,
                    Quaternion.CreateFromAxisAngle(
                        Vector3.UnitY,
                        yaw));

            Vector3 direction =
                forwardDirection * forward +
                rightDirection * right;

            direction.Y = 0f;

            if (direction.LengthSquared() >
                0.0001f)
            {
                direction =
                    Vector3.Normalize(direction);
            }

            return direction;
        }

        // ============================================================
        // Speed
        // ============================================================

        private float GetMovementSpeed()
        {
            if (AllowSprint &&
                Engine.InputSystem.GetAction("Sprint") > 0.5f)
            {
                return SprintSpeed;
            }

            return WalkSpeed;
        }

        private float HorizontalSpeed()
        {
            Vector3 horizontal =
                new(
                    rb.LinearVelocity.X,
                    0f,
                    rb.LinearVelocity.Z);

            return horizontal.Length();
        }

        // ============================================================
        // Jumping
        // ============================================================

        private void HandleJump()
        {
            if (!jumpQueued || !canJump)
                return;

            jumpQueued = false;

            Vector3 velocity = rb.LinearVelocity;

            velocity.Y = JumpVelocity;

            rb.LinearVelocity = velocity;

            ChangeState(MovementState.Air);

            canJump = false;
            coyoteTimer = 0f;
        }

        private void UpdateCoyoteTime(float dt)
        {
            if (IsGrounded())
            {
                coyoteTimer = CoyoteTime;
                canJump = true;
                return;
            }

            coyoteTimer -= dt;

            if (coyoteTimer <= 0f)
            {
                canJump = false;
            }
        }

        // ============================================================
        // Ground Detection
        // ============================================================

        private void UpdateGrounded(float dt)
        {
            bool grounded = TryGetGroundNormal(out Vector3 _);

            if (grounded)
            {
                if (currentState == MovementState.Air)
                {
                    ChangeState(
                        MovementState.Ground);
                }

                canJump = true;
                coyoteTimer = CoyoteTime;
            }
            else
            {
                GroundToAir();
            }
        }

        private bool IsGrounded()
        {
            return TryGetGroundNormal(out _);
        }

        private bool TryGetGroundNormal(out Vector3 normal)
        {
            normal = Vector3.UnitY;

            float skin = 0.05f;

            Vector3 origin = gameObject.Transform.Position +
                Vector3.UnitY * 0.1f;

            float rayLength =
                (CapsuleHeight * 0.6f) +
                CapsuleRadius +
                skin;

            Ray ray = new(origin, Vector3.UnitY * -1);


            if (!gameObject.Scene!.Physics.Raycast(ray, rayLength, out RaycastHit hit, new IgnoreGameObjectRaycastFilter(gameObject)))
            {

                return false;
            }

            normal = hit.Normal;


            return normal.Y >= GroundNormalThreshold;
        }

        // ============================================================
        // Wall Detection
        // ============================================================

        private bool TryGetWallNormal(
            Vector3 direction,
            out Vector3 normal)
        {
            normal = Vector3.Zero;

            if (direction.LengthSquared() <
                0.0001f)
            {
                direction =
                    new Vector3(
                        rb.LinearVelocity.X,
                        0f,
                        rb.LinearVelocity.Z);

                if (direction.LengthSquared() <
                    0.0001f)
                {
                    return false;
                }
            }

            direction.Y = 0f;

            direction =
                Vector3.Normalize(direction);

            Vector3 origin =
                gameObject.Transform.Position +
                Vector3.UnitY *
                (CapsuleHeight * 0.5f);

            Ray ray =
                new(
                    origin,
                    direction);

            if (!gameObject.Scene!.Physics.Raycast(
                    ray,
                    CapsuleRadius +
                    WallDetectionDistance,
                    out RaycastHit hit))
            {
                return false;
            }

            if (hit.HitObject == gameObject)
                return false;

            // Ignore floors and ceilings.
            if (MathF.Abs(hit.Normal.Y) >
                0.5f)
            {
                return false;
            }

            normal =
                Vector3.Normalize(hit.Normal);

            return true;
        }

        // ============================================================
        // Ground -> Air
        // ============================================================

        private bool GroundToAir()
        {
            if (IsGrounded())
                return false;

            ChangeState(
                MovementState.Air);

            if (canJump)
                coyoteTimer =
                    CoyoteTime;

            return true;
        }

        // ============================================================
        // Sliding
        // ============================================================

        private void ToSlide()
        {
            if (canSlideBoost)
            {
                Vector3 velocity =
                    rb.LinearVelocity;

                Vector3 horizontal =
                    new(
                        velocity.X,
                        0f,
                        velocity.Z);

                if (horizontal.LengthSquared() >
                    0.0001f)
                {
                    ApplyForce(
                        Vector3.Normalize(horizontal) *
                        SlideBoostForce);
                }

                canSlideBoost = false;

                _ = ResetSlideBoost();
            }

            slideCurvePoint = 0f;

            ChangeState(
                MovementState.Sliding);
        }

        private async Task ResetSlideBoost()
        {
            await Task.Delay(
                TimeSpan.FromSeconds(
                    SlideBoostTime));

            canSlideBoost = true;
        }

        // ============================================================
        // Wall Run
        // ============================================================

        private bool IsWallRunningLeft(
            Vector3 wallNormal)
        {
            Vector3 right =
                Vector3.Transform(
                    Vector3.UnitX,
                    Quaternion.CreateFromAxisAngle(
                        Vector3.UnitY,
                        yaw));

            return Vector3.Dot(
                wallNormal,
                right) < 0f;
        }

        private void ResetWallRun()
        {
            if (hasLeftWallRun)
            {
                _ = ResetLeftWallRun();
            }

            if (hasRightWallRun)
            {
                _ = ResetRightWallRun();
            }

            if (currentState !=
                MovementState.WallRunning)
            {
                wallRunPoint = 0f;
            }

            previousWallNormal =
                Vector3.UnitY;
        }

        private async Task ResetLeftWallRun()
        {
            await Task.Delay(
                TimeSpan.FromSeconds(
                    WallRunResetTime));

            hasLeftWallRun = false;
        }

        private async Task ResetRightWallRun()
        {
            await Task.Delay(
                TimeSpan.FromSeconds(
                    WallRunResetTime));

            hasRightWallRun = false;
        }

        // ============================================================
        // State
        // ============================================================

        private void ChangeState(
            MovementState newState)
        {
            if (currentState == newState)
                return;

            previousState =
                currentState;

            currentState =
                newState;

            if (previousState ==
                MovementState.Sliding)
            {
                slideCurvePoint = 0f;
            }

            if (currentState ==
                MovementState.WallRunning)
            {
                wallRunPoint = 0f;
            }

            if (previousState ==
                MovementState.WallRunning)
            {
                wallRunPoint = 0f;
            }
        }

        // ============================================================
        // Forces
        // ============================================================

        private void ApplyForce(
            Vector3 force)
        {
            rb.LinearVelocity += force;
        }

        private void SlowMovement(
            float amount)
        {
            rb.LinearVelocity *= amount;
        }

        // ============================================================
        // Angle
        // ============================================================

        private float GetHorizontalAngle(
            Vector3 vec1,
            Vector3 vec2)
        {
            vec1.Y = 0f;
            vec2.Y = 0f;

            if (vec1.LengthSquared() <
                0.0001f ||
                vec2.LengthSquared() <
                0.0001f)
            {
                return 0f;
            }

            vec1 =
                Vector3.Normalize(vec1);

            vec2 =
                Vector3.Normalize(vec2);

            float dot =
                Math.Clamp(
                    Vector3.Dot(
                        vec1,
                        vec2),
                    -1f,
                    1f);

            return MathF.Acos(dot);
        }

        // ============================================================
        // Air Strafe Curve
        // ============================================================

        private float AirStrafeCurve(
            float x)
        {
            x = Math.Clamp(
                x,
                0f,
                1f);

            // Smooth curve approximation.
            //
            // 0 -> 0
            // 0.5 -> ~0.5
            // 1 -> 1

            return x * x *
                   (3f - 2f * x);
        }


        // ============================================================
        // Public API
        // ============================================================

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

        public GameObject GetHoldPoint()
        {
            return holdPoint;
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