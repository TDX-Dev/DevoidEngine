using DevoidEngine.Engine.Core;

namespace DevoidEngine.Engine.Physics
{
    public interface ICollisionListener
    {
        void OnCollisionEnter(GameObject other);
        void OnCollisionStay(GameObject other);
        void OnCollisionExit(GameObject other);
    }
}