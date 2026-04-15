using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;

namespace ElementalEditor.Scripting
{
    public static class ScriptComponentReload
    {
        public class SavedComponent
        {
            public GameObject GameObject;
            public Component OldComponent;
            public string TypeName;
        }

        public static List<SavedComponent> RemoveScriptComponents(Scene scene)
        {
            List<SavedComponent> saved = new();

            foreach (var go in scene.GameObjects)
            {
                for (int i = go.Components.Count - 1; i >= 0; i--)
                {
                    var comp = go.Components[i];

                    if (comp.GetType().Assembly == ScriptAssemblyLoader.Assembly)
                    {
                        saved.Add(new SavedComponent
                        {
                            GameObject = go,
                            OldComponent = comp,
                            TypeName = comp.GetType().FullName!
                        });

                        go.RemoveComponent(comp);
                    }
                }
            }

            return saved;
        }

        public static void RestoreScriptComponents(List<SavedComponent> saved)
        {
            var asm = ScriptAssemblyLoader.Assembly;

            foreach (var entry in saved)
            {
                var type = asm.GetType(entry.TypeName);

                if (type == null)
                    continue;

                var newComp = (Component)Activator.CreateInstance(type)!;
                entry.GameObject.AddComponent(newComp);

                ScriptStateTransfer.CopyState(entry.OldComponent, newComp);
            }
        }
    }
}