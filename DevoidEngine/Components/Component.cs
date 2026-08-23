using DevoidEngine.Core;
using DevoidEngine.Serialization;
using System.Diagnostics;

namespace DevoidEngine.Components
{
    [Flags]
    public enum ComponentTickMode
    {
        None = 0,

        Play = 1 << 0,
        Edit = 1 << 1,

        All = Play | Edit
    }

    public abstract partial class Component
    {
        public override string ToString() => Type;
        public virtual ComponentTickMode TickMode => ComponentTickMode.Play;

        [DontSerialize]
        public bool IsInitialized;

        public abstract string Type { get; }
        public Component() { }

        public GameObject gameObject = null!;

        private bool CanTick
        {
            get
            {
                if (gameObject == null || gameObject.Scene == null)
                    return false;

                ComponentTickMode mode = TickMode;

                return gameObject.Scene.SceneMode switch
                {
                    SceneMode.Play =>
                        (mode & ComponentTickMode.Play) != 0,

                    SceneMode.Edit =>
                        (mode & ComponentTickMode.Edit) != 0,

                    _ =>
                        false
                };
            }
        }


        public void InternalStart()
        {
            if (IsInitialized)
                return;

            if (!CanTick)
                return;

            OnStart();
            IsInitialized = true;
        }

        public void InternalUpdate(float dt)
        {
            if (!CanTick)
                return;

            OnUpdate(dt);
        }

        public void InternalLateUpdate(float dt)
        {
            if (!CanTick)
                return;

            OnLateUpdate(dt);
        }

        public void InternalFixedUpdate(float dt)
        {
            if (!CanTick)
                return;

            OnFixedUpdate(dt);
        }

        public void InternalRender()
        {
            if (!CanTick)
                return;

            OnRender();
        }

        public void InternalDestroy()
        {

            OnDestroy();
            IsInitialized = false;
        }
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
