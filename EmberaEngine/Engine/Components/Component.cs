using System;
using EmberaEngine.Engine.Core;

namespace EmberaEngine.Engine.Components
{
    public abstract class Component
    {
        public abstract string Type { get; }
        public Component()
        {

        }

        public GameObject gameObject;
        public virtual void OnStart() { }
        public virtual void OnUpdate(float dt) { }

        public virtual void OnDestroy() { }

    }
}

