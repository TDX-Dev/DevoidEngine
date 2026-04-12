using DevoidEngine.Engine.Components;

namespace DevoidEngine.Engine.Components;

public abstract class ScriptComponent : Component
{
    public override string Type => GetType().Name;
}