using DevoidEngine.InputSystem.InputDevices;
using DevoidEngine.Logging;

namespace Elemental.Tools.Shortcuts
{
    public class ShortcutManager
    {
        private readonly Dictionary<Shortcut, EditorActionId> _bindings;

        public ShortcutManager()
        {
            _bindings = [];
        }

        public void Register(Shortcut shortcut, EditorActionId id)
        {
            if (_bindings.ContainsKey(shortcut))
            {
                DevoidLog.Warning(LogCategory.Editor, "Tried to add shortcut duplicate.");
                return;
            }

            _bindings.Add(shortcut, id);
        }

        public void Unregister(Shortcut shortcut)
        {
            if (!_bindings.Remove(shortcut))
                DevoidLog.Warning(LogCategory.Editor, "Tried to remove shortcut when none existed.");
        }

        public bool TryGetAction(Keys key, KeyModifiers modifiers, out EditorActionId action)
        {
            return _bindings.TryGetValue(new Shortcut(modifiers, key), out action);
        }

    }
}
