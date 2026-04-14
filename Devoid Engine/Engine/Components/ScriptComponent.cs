using DevoidEngine.Engine.Core;

namespace DevoidEngine.Engine.Components
{
    public class ScriptComponent : Component
    {
        public override string Type => "Script";

        public string? ScriptType = "";
        public ScriptBehaviour Behaviour = null!;

        public void Bind(ScriptBehaviour behaviour)
        {
            Behaviour = behaviour;
            Behaviour.gameObject = gameObject;
        }

        public override void OnStart()
        {
            Behaviour?.OnStart();
        }

        public override void OnUpdate(float dt)
        {
            Console.WriteLine("ScriptComponent updating" + (Behaviour == null));
            Behaviour?.OnUpdate(dt);
        }

        public override void OnLateUpdate(float dt)
        {
            Behaviour?.OnLateUpdate(dt);
        }

        public override void OnFixedUpdate(float dt)
        {
            Behaviour?.OnFixedUpdate(dt);
        }

        public override void OnRender()
        {
            Behaviour?.OnRender();
        }

        public override void OnDestroy()
        {
            Behaviour?.OnDestroy();
        }
    }
}