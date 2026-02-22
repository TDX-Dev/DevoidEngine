using DevoidEngine.Engine.Core;

namespace DevoidEngine.Engine.Components
{
    public abstract partial class Component
    {
        public bool IsInitialized;
        public abstract string Type { get; }
        public Component() { }

        public GameObject gameObject;
        public virtual void OnStart() { }
        public virtual void OnUpdate(float dt) { }
        public virtual void OnLateUpdate(float dt) { }
        public virtual void OnFixedUpdate(float dt) { }
        public virtual void OnRender(float dt) { }

        public virtual void OnDestroy()
        {

        }
    }
}

