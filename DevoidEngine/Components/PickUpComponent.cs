using DevoidEngine.Components.PickUpBehavior;
using DevoidEngine.Core;
using DevoidEngine.InputSystem;
using DevoidEngine.Physics;
using DevoidEngine.Util;
using System.Numerics;

namespace DevoidEngine.Components
{
    public class PickupComponent : Component
    {
        public override string Type => nameof(PickupComponent);

        public float PickupDistance = 5f;
        public float HoldSpring = 950f;
        public float HoldDamping = 40f;

        public float HoldAngularSpring = 950f;
        public float HoldAngularDamping = 40f;

        private GameObject? heldObject;
        private PickupItem? heldItem;

        private Camera3D? camera;
        private GameObject? holdPoint;
        private FirstPersonController? controller;

        public bool IsHolding => heldObject != null;
        public GameObject? HeldObject => heldObject;

        public void SetCamera(Camera3D camera)
        {
            this.camera = camera;
        }
        public void SetHoldPoint(GameObject holdPoint)
        {
            this.holdPoint = holdPoint;
        }
        public void SetController(FirstPersonController controller)
        {
            this.controller = controller;
        }

        public override void OnUpdate(float dt)
        {
            if (camera == null)
                return;

            if (Engine.InputSystem.GetActionDown("Pickup"))
                TryPickup();

            if (heldItem == null)
                return;

            if (Engine.InputSystem.GetActionDown("Primary"))
                heldItem.OnPrimary();

            if (Engine.InputSystem.GetActionDown("Secondary"))
                heldItem.OnSecondary();

            if (Engine.InputSystem.GetActionDown("Use"))
                heldItem.OnUse();
        }

        public override void OnFixedUpdate(float dt)
        {
            if (heldObject == null ||
                heldItem == null ||
                holdPoint == null)
            {
                return;
            }

            RigidBodyComponent? rb = heldObject.GetComponent<RigidBodyComponent>();

            if (rb == null || controller == null)
                return;

            // ============================================================
            // Position spring
            // ============================================================

            Vector3 targetPosition =
                holdPoint.Transform.Position;

            Vector3 currentPosition =
                heldObject.Transform.Position;

            Vector3 positionError =
                targetPosition - currentPosition;

            Vector3 springForce =
                positionError * HoldSpring;

            Vector3 dampingForce =
                -rb.LinearVelocity * HoldDamping;

            rb.LinearVelocity +=
                (springForce + dampingForce) * dt;


            // ============================================================
            // Rotation spring
            // ============================================================

            // ============================================================
            // Rotation spring
            // ============================================================

            Vector3 offset =  heldItem.HoldEulerOffset;

            Quaternion offsetRotation =
                Quaternion.CreateFromYawPitchRoll(
                    offset.Y,
                    offset.X,
                    offset.Z);

            Quaternion yawRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, controller.GetYaw());

            Quaternion targetRotation =
                Quaternion.Normalize(
                    yawRotation *
                    offsetRotation);

            Quaternion currentRotation =
                heldObject.Transform.Rotation;

            // Rotation needed to go from current -> target.
            Quaternion rotationDifference =
                targetRotation *
                Quaternion.Inverse(currentRotation);

            rotationDifference =
                Quaternion.Normalize(rotationDifference);

            // Take the shortest path.
            if (rotationDifference.W < 0f)
            {
                rotationDifference.X *= -1f;
                rotationDifference.Y *= -1f;
                rotationDifference.Z *= -1f;
                rotationDifference.W *= -1f;
            }

            // Quaternion:
            // q = [axis * sin(angle / 2), cos(angle / 2)]

            float w =
                Math.Clamp(
                    rotationDifference.W,
                    -1f,
                    1f);

            float angle =
                2f * MathF.Acos(w);

            float sinHalfAngle =
                MathF.Sqrt(
                    MathF.Max(
                        0f,
                        1f - w * w));

            Vector3 axis;

            if (sinHalfAngle > 0.0001f)
            {
                axis = new Vector3(
                    rotationDifference.X,
                    rotationDifference.Y,
                    rotationDifference.Z);

                axis /= sinHalfAngle;
            }
            else
            {
                axis = Vector3.Zero;
            }

            if (axis.LengthSquared() > 0.0001f)
            {
                axis = Vector3.Normalize(axis);

                Vector3 angularSpring =
                    axis *
                    angle *
                    HoldAngularSpring;

                Vector3 angularDamping =
                    -rb.AngularVelocity *
                    HoldAngularDamping;

                rb.AngularVelocity +=
                    (angularSpring + angularDamping) * dt;
            }
            else
            {
                rb.AngularVelocity = Vector3.Zero;
            }
        }

        public void TryPickup()
        {
            if (heldObject != null)
            {
                Drop();
                return;
            }

            if (!TryFindPickup(out GameObject? target))
                return;

            PickupItem? item = target?.GetComponent<PickupItem>();

            if (item == null || !item.CanPickup)
                return;

            heldObject = target;
            heldItem = item;

            RigidBodyComponent? rb = heldObject?.GetComponent<RigidBodyComponent>();

            if (rb != null)
            {
                rb.LinearVelocity = Vector3.Zero;
                rb.AngularVelocity = Vector3.Zero;
            }

            heldItem.HeldCamera = camera;
            heldItem.HeldBy = gameObject;
            heldItem.OnPickup(gameObject);
        }

        public void Drop()
        {
            if (heldItem == null)
                return;

            heldItem.OnDrop();

            heldItem = null;
            heldObject = null;
        }

        private bool TryFindPickup(out GameObject? target)
        {
            target = null;

            if (camera == null)
                return false;

            Vector3 origin = camera.gameObject.Transform.Position;

            Vector3 direction = camera.gameObject.Transform.Forward;

            Ray ray = new(origin, direction);

            if (!gameObject.Scene!.Physics.Raycast(ray, PickupDistance, out RaycastHit hit, new IgnoreGameObjectRaycastFilter(gameObject)))
            {
                return false;
            }



            target = hit.HitObject;

            Console.WriteLine(target.Name);

            return target != null;
        }
    }
}