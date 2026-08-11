using DevoidEngine.Core;
using DevoidEngine.Physics;
using DevoidEngine.Util;
using System.Numerics;

namespace DevoidEngine.Components
{
    public class PickupComponent : Component
    {
        public override string Type => nameof(PickupComponent);

        public float PickupDistance = 5.0f;
        public float HoldDistance = 3.0f;

        public Vector3 HoldOffset = new(1, 0, 2);

        public float SpringStrength = 420.0f;
        public float Damping = 17.5f;
        public float MaxForce = 750;

        public float ThrowHoldTime = 1.0f;
        public float ThrowForce = 35.0f;

        public float ShakeAmplitude = 0.08f;
        public float ShakeFrequency = 18.0f;

        private RigidBodyComponent? _heldBody;

        private Camera3D _camera = null!;

        private float _holdTime;

        private bool _isCharging;

        public bool IsHolding => _heldBody != null;

        public float MaxSpinSpeed = 60.0f;
        public float SpinRampPower = 4.0f;
        public Vector3 SpinAxis = new(0.3f, 1.0f, 0.2f);

        public void SetCamera(Camera3D camera)
        {
            _camera = camera;
        }

        public void TryPickup()
        {
            if (_heldBody != null)
            {
                _isCharging = true;
                _holdTime = 0.0f;
                return;
            }

            Ray ray = new(
                _camera.gameObject.Transform.Position,
                _camera.gameObject.Transform.Forward);

            var filter =
                new IgnoreGameObjectRaycastFilter(gameObject);

            bool hitSomething =
                gameObject.Scene!.Physics.Raycast(
                    ray,
                    PickupDistance,
                    out RaycastHit hit,
                    filter);

            if (!hitSomething)
                return;

            RigidBodyComponent? body =
                hit.HitObject.GetComponent<RigidBodyComponent>();

            if (body == null)
                return;

            _heldBody = body;

            _isCharging = false;
            _holdTime = 0.0f;
        }

        public void Release()
        {
            _heldBody = null;
            _isCharging = false;
            _holdTime = 0.0f;
        }

        public override void OnUpdate(float dt)
        {
            if (_heldBody == null)
                return;

            UpdateHeldObject(dt);

            if (!_isCharging)
                return;

            _holdTime += dt;

            bool held =
                Engine.InputSystem.GetAction("Pickup") > 0.5f;

            if (!held)
            {
                if (_holdTime >= ThrowHoldTime)
                    Throw();
                else
                    Release();
            }
        }

        private void UpdateHeldObject(float dt)
        {
            Vector3 shake = CalculateShake();

            Vector3 localOffset =
                HoldOffset + shake;

            Vector3 targetPosition =
                _camera.gameObject.Transform.Position +
                Vector3.Transform(
                    localOffset,
                    _camera.gameObject.Transform.Rotation);

            Vector3 currentPosition =
                _heldBody!.gameObject.Transform.Position;

            Vector3 error =
                targetPosition - currentPosition;

            Vector3 velocity =
                _heldBody.LinearVelocity;

            Vector3 acceleration =
                error * SpringStrength -
                velocity * Damping;

            float maxAcceleration = MaxForce;

            if (acceleration.LengthSquared() >
                maxAcceleration * maxAcceleration)
            {
                acceleration =
                    Vector3.Normalize(acceleration) *
                    maxAcceleration;
            }

            velocity += acceleration * dt;

            _heldBody.LinearVelocity = velocity;

            if (_isCharging)
            {
                float charge =
                    Math.Clamp(
                        _holdTime / ThrowHoldTime,
                        0.0f,
                        1.0f);

                float spin =
                    MathF.Pow(charge, SpinRampPower);

                Vector3 axis = SpinAxis;

                if (axis.LengthSquared() > 0.0001f)
                    axis = Vector3.Normalize(axis);

                _heldBody.AngularVelocity =
                    axis * (MaxSpinSpeed * spin);
            }
        }

        private Vector3 CalculateShake()
        {
            float t = _holdTime;

            float x =
                MathF.Sin(t * ShakeFrequency) *
                ShakeAmplitude;

            float y =
                MathF.Cos(t * ShakeFrequency * 1.37f) *
                ShakeAmplitude;

            float z =
                MathF.Sin(t * ShakeFrequency * 0.83f) *
                ShakeAmplitude;

            return new Vector3(x, y, z);
        }

        private void Throw()
        {
            Vector3 throwVelocity =
                _camera.gameObject.Transform.Forward *
                ThrowForce;

            _heldBody!.LinearVelocity += throwVelocity;

            Release();
        }
    }
}