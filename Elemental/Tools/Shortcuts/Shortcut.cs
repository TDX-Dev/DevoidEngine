using DevoidEngine.InputSystem.InputDevices;

namespace Elemental.Tools.Shortcuts
{
    public readonly record struct Shortcut
    {

        public KeyModifiers Modifiers { get; }
        public Keys Key { get; }

        public Shortcut(KeyModifiers modifiers, Keys key)
        {
            Modifiers = modifiers & (
                KeyModifiers.Shift |
                KeyModifiers.Control |
                KeyModifiers.Alt |
                KeyModifiers.Super);

            Key = key;
        }

        public bool HasModifier(KeyModifiers modifier) => (Modifiers & modifier) != 0;

        public bool Matches(Keys key, KeyModifiers modifiers) => Key == key && Modifiers == modifiers;

        public override string ToString()
        {
            var result = "";

            if (HasModifier(KeyModifiers.Control))
                result += "Ctrl+";

            if (HasModifier(KeyModifiers.Shift))
                result += "Shift+";

            if (HasModifier(KeyModifiers.Alt))
                result += "Alt+";

            if (HasModifier(KeyModifiers.Super))
                result += "Super+";

            return result + Key;
        }
    }
}
