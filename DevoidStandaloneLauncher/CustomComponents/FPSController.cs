using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Physics;
using DevoidEngine.Engine.UI.Nodes;
using DevoidEngine.Engine.UI.Text;
using DevoidEngine.Engine.Utilities;
using System;
using System.Numerics;

namespace DevoidEngine.Engine.Components
{
    public class FPSController : Component, ICollisionListener
    {
        public override string Type => nameof(FPSController);

        // ===============================
        // Movement
        // ===============================

        public float MoveSpeed = 6f;
        public float Acceleration = 20f;
        public float AirControl = 0.3f;
        public float JumpForce = 100f;
        public float MouseSensitivity = 0.12f;
        public float MinPitch = -89f;
        public float MaxPitch = 89f;
        public float GroundCheckDistance = 5f;

        // ===============================
        // Shooting / Ammo
        // ===============================

        public float FireRate = 0.05f;
        public float ProjectileSpeed = 40f;
        public float ProjectileMass = 0.2f;
        public Vector3 ProjectileScale = new Vector3(0.2f);

        public int MaxAmmo = 30;
        public float ReloadTime = 1.5f;

        public int currentAmmo;
        public bool isReloading = false;
        private float reloadTimer = 0f;
        private float fireTimer = 0f;

        private Mesh projectileMesh;

        // ===============================
        // Health + Regen
        // ===============================

        public float MaxHealth = 100f;
        public float Health = 100f;

        public float EnterDamage = 15f;
        public float DamagePerSecond = 25f;
        public float DamageDelay = 1f;

        public float RegenPerSecond = 10f;
        public float RegenDelay = 3f;

        private float lastDamageTime = 0f;
        private float damageTimer = 0f;
        private bool inDamageZone = false;
        private bool delayPassed = false;

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

        public event Action OnDeath;

        public float InteractionDistance = 20f;

        public CanvasComponent UICanvas;
        public LabelNode UIInteractionText;

        private PortalCubeComponent heldCube;

        // ===============================
        // Setup
        // ===============================

        private Vector2 lastMouseDelta;
        private Vector2 mouseVelocity;

        public Vector2 GetMouseVelocity() => mouseVelocity;
        public Transform GetCameraPivot() => cameraPivot;
        public Vector2 GetMouseDelta() => lastMouseDelta;
        public Vector3 GetLinearVelocity()
        {
            return rb != null ? rb.LinearVelocity : Vector3.Zero;
        }

        public override void OnStart()
        {
            GameObject canvas = gameObject.Scene.addGameObject("InteractionObject");
            UICanvas = canvas.AddComponent<CanvasComponent>();

            FlexboxNode ROOT = new FlexboxNode()
            {
                Size = new Vector2(200,600),
                Justify = JustifyContent.Center,
                Align = AlignItems.End
            };

            UIInteractionText = new LabelNode("T to interact", FontLibrary.LoadFont("Engine/Content/Fonts/JetBrainsMono-Regular.ttf", 32), 21)
            {

            };
            ROOT.Add(UIInteractionText);
            UICanvas.Canvas.Add(ROOT);

            projectileMesh = new Mesh();
            projectileMesh.SetVertices(Primitives.GetCubeVertex());

            currentAmmo = MaxAmmo;

            rb = gameObject.GetComponent<RigidBodyComponent>();
            if (rb == null)
                return;

            rb.FreezeRotationX = true;
            rb.FreezeRotationY = true;
            rb.FreezeRotationZ = true;

            yaw = MathHelper.RadToDeg(
                MathF.Atan2(
                    gameObject.transform.Forward.X,
                    gameObject.transform.Forward.Z
                )
            );
        }

        public void SetCameraPivot(Transform pivot)
        {
            cameraPivot = pivot;
        }

        // ===============================
        // Update
        // ===============================

        float totalTime = 0f;

        public override void OnUpdate(float dt)
        {
            lastMouseDelta = Input.MouseDelta;
            mouseVelocity = lastMouseDelta / dt;

            totalTime += dt;
            if (rb == null) return;

            moveInput = Input.MoveAxis;
            mouseDelta += Input.MouseDelta;

            if (Input.JumpPressed)
                jumpRequested = true;

            fireTimer -= dt;

            HandleReload(dt);

            if (Input.GetMouseDown(MouseButton.Left))
                TryShoot();

            if (Input.GetKey(Keys.E))
                StartReload();

            HandleDamage(dt);
            HandleRegen(dt);


            TryInteract();
                
        }

        private void TryInteract()
        {
            if (cameraPivot == null)
                return;

            Vector3 origin = cameraPivot.Position;

            Vector3 forward =
                Vector3.Normalize(
                    Vector3.Transform(Vector3.UnitZ, cameraPivot.Rotation)
                );

            if (Input.GetKeyDown(Keys.Q))
            {
                // If holding cube -> drop
                if (heldCube != null)
                {
                    heldCube.Drop();
                    heldCube = null;
                    return;
                }

                // Try pickup
                if (gameObject.Scene.Physics.Raycast(
                    new Ray(origin + forward, forward),
                    InteractionDistance,
                    out RaycastHit hit))
                {
                    var cube = hit.HitObject?.GetComponent<PortalCubeComponent>();

                    if (cube != null && !cube.IsHeld)
                    {
                        heldCube = cube;
                        cube.PickUp(this);
                    }
                }
            }
        }

        // ===============================
        // Shooting
        // ===============================

        private void TryShoot()
        {
            if (fireTimer > 0f || isReloading)
                return;

            if (currentAmmo <= 0)
            {
                StartReload();
                return;
            }

            fireTimer = FireRate;
            currentAmmo--;

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

        private void StartReload()
        {
            if (isReloading || currentAmmo == MaxAmmo)
                return;

            isReloading = true;
            reloadTimer = ReloadTime;
        }

        private void HandleReload(float dt)
        {
            if (!isReloading)
                return;

            reloadTimer -= dt;

            if (reloadTimer <= 0f)
            {
                isReloading = false;
                currentAmmo = MaxAmmo;
            }
        }

        // ===============================
        // Health + Damage
        // ===============================

        private void HandleDamage(float dt)
        {
            if (!inDamageZone)
                return;

            damageTimer += dt;

            if (!delayPassed && damageTimer >= DamageDelay)
                delayPassed = true;

            if (delayPassed)
                ApplyDamage(DamagePerSecond * dt);
        }

        private void HandleRegen(float dt)
        {
            if (inDamageZone)
                return;

            if (TimeSinceLastDamage() < RegenDelay)
                return;

            if (Health < MaxHealth)
            {
                Health += RegenPerSecond * dt;
                Health = Math.Min(Health, MaxHealth);
            }
        }

        private float TimeSinceLastDamage()
        {
            return (float)(totalTime - lastDamageTime);
        }

        private void ApplyDamage(float amount)
        {
            Health -= amount;
            lastDamageTime = (float)totalTime;

            if (Health <= 0f)
            {
                Health = 0f;
                OnDeath?.Invoke();
            }
        }

        // ===============================
        // Physics
        // ===============================

        public override void OnFixedUpdate(float fixedDt)
        {
            if (rb == null) return;

            HandleRotation();
            HandleMovement(fixedDt);

            jumpRequested = false;
        }

        private void HandleRotation()
        {
            yaw += mouseDelta.X * MouseSensitivity;
            pitch -= mouseDelta.Y * MouseSensitivity;

            pitch = Math.Clamp(pitch, MinPitch, MaxPitch);

            rb.Rotation =
                Quaternion.CreateFromAxisAngle(
                    -Vector3.UnitY,
                    MathHelper.DegToRad(yaw)
                );

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

        private void HandleMovement(float fixedDt)
        {
            Vector3 forward = Vector3.Transform(Vector3.UnitZ, rb.Rotation);
            Vector3 right = Vector3.Transform(-Vector3.UnitX, rb.Rotation);

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

            if (jumpRequested && grounded)
            {
                rb.LinearVelocity = new Vector3(
                    rb.LinearVelocity.X,
                    JumpForce,
                    rb.LinearVelocity.Z
                );
            }
        }

        private bool IsGrounded()
        {
            Vector3 origin = gameObject.transform.Position;

            return gameObject.Scene.Physics.Raycast(
                new Ray(origin, -Vector3.UnitY),
                GroundCheckDistance,
                out RaycastHit hit
            );
        }

        // ===============================
        // Collision Events
        // ===============================

        public void OnCollisionEnter(GameObject other)
        {
            if (other.Name == "Ground") return;
            //Console.WriteLine("ENTER");

            inDamageZone = true;
            damageTimer = 0f;
            delayPassed = false;

            ApplyDamage(EnterDamage);
        }

        public void OnCollisionStay(GameObject other) { }

        public void OnCollisionExit(GameObject other)
        {
            if (other.Name == "Ground") return;
            inDamageZone = false;
            damageTimer = 0f;
            delayPassed = false;
        }
    }
}