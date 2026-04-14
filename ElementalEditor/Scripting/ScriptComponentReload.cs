using DevoidEngine.Engine.Core;

namespace ElementalEditor.Scripting
{
    public static class ScriptComponentReload
    {
        public static void RemoveScriptComponents(Scene scene)
        {
            foreach (var go in scene.GameObjects)
            {
                for (int i = go.Components.Count - 1; i >= 0; i--)
                {
                    var comp = go.Components[i];

                    var asm = comp.GetType().Assembly;

                    if (asm == ScriptAssemblyLoader.Assembly)
                    {
                        go.RemoveComponent(comp);
                    }
                }
            }
        }
    }
}
