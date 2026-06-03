using DevoidEngine.Core;
using DevoidEngine.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Components
{
    public abstract partial class Component
    {
        public override string ToString() => Type;

        [DontSerialize]
        public bool IsInitialized;

        internal void InternalStart()
        {
            if (IsInitialized)
            {
                Console.WriteLine($"{Type} Component started twice. Skipping component start.");
            }
            OnStart();
            IsInitialized = true;
        }

        public abstract string Type { get; }
        public Component() { }

        public GameObject gameObject = null!;

        // Notifications methods
        public virtual void OnAttach() { }

        public virtual void OnStart() { }
        public virtual void OnUpdate(float dt) { }
        public virtual void OnLateUpdate(float dt) { }
        public virtual void OnFixedUpdate(float dt) { }
        public virtual void OnRender() { }

        public virtual void OnDestroy()
        {

        }

        public SceneTree GetTree() => Engine.Instance.SceneTree;
    }
}
