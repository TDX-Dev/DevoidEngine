using DevoidEngine.InputSystem.InputDevices;
using DevoidEngine.Logging;

namespace Elemental.Tools.Shortcuts
{
    public class ShortcutManager
    {
        private readonly Dictionary<Shortcut, EditorActionId> bindings;

        public ShortcutManager()
        {
            bindings = [];
        }

        public void Register(Shortcut shortcut, EditorActionId id)
        {
            if (bindings.ContainsKey(shortcut))
            {
                DevoidLog.Warning(LogCategory.Editor, "Tried to add shortcut duplicate.");
                return;
            }

            bindings.Add(shortcut, id);
        }

        public void Unregister(Shortcut shortcut)
        {
            if (!bindings.Remove(shortcut))
                DevoidLog.Warning(LogCategory.Editor, "Tried to remove shortcut when none existed.");
        }

        public bool TryGetAction(Keys key, KeyModifiers modifiers, out EditorActionId action)
        {
            return bindings.TryGetValue(new Shortcut(modifiers, key), out action);
        }

    }
}
