using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Serialization;

namespace DevoidEngine.Engine.Components
{
    public abstract partial class Component
    {
        [DontSerialize]
        public bool IsInitialized;

        internal void InternalStart()
        {


            OnStart();
            IsInitialized = true;
        }

        public abstract string Type { get; }
        public Component() { }

        public GameObject gameObject = null!;
        public virtual void OnStart() { }
        public virtual void OnUpdate(float dt) { }
        public virtual void OnLateUpdate(float dt) { }
        public virtual void OnFixedUpdate(float dt) { }
        public virtual void OnRender() { }

        public virtual void OnDestroy()
        {

        }
    }
}