using DevoidEngine.Core;

namespace DevoidEngine.Physics
{
    public interface ICollisionListener
    {
        void OnCollisionEnter(GameObject other);
        void OnCollisionStay(GameObject other);
        void OnCollisionExit(GameObject other);
    }
}
