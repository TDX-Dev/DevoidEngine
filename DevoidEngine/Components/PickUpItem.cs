using DevoidEngine.Core;
using System.Numerics;

namespace DevoidEngine.Components
{
    public abstract class PickupItem : Component
    {
        public bool CanPickup = true;
        public Vector3 HoldEulerOffset = Vector3.Zero;

        public GameObject? HeldBy;
        public Camera3D? HeldCamera;

        public virtual void OnPickup(GameObject player)
        {
        }

        public virtual void OnDrop()
        {
        }

        public virtual void OnUse()
        {
        }

        public virtual void OnPrimary()
        {
        }

        public virtual void OnSecondary()
        {
        }
    }
}