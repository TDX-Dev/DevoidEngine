using DevoidEngine.Engine.InputSystem;
using DevoidEngine.Engine.InputSystem.InputDevices;
using DevoidEngine.Engine.ProjectSystem;
using ImGuiNET;

namespace ElementalEditor.ProjectSettings
{
    public class InputSettingsProvider : IProjectSettingsProvider
    {
        public string Category => "Input System";
        public string Name => "Input";

        string newActionName = "";

        public void Draw()
        {
            var settings = ProjectManager.Current.Settings;

            //--------------------------------
            // Add new action
            //--------------------------------

            ImGui.InputText("New Action", ref newActionName, 64);

            ImGui.SameLine();

            if (ImGui.Button("Add Action") && !string.IsNullOrWhiteSpace(newActionName))
            {
                settings.InputActions.Add(new InputAction
                {
                    Name = newActionName
                });

                newActionName = "";
            }

            ImGui.Separator();

            //--------------------------------
            // Existing actions
            //--------------------------------

            for (int i = 0; i < settings.InputActions.Count; i++)
            {
                var action = settings.InputActions[i];

                ImGui.PushID(i);

                bool open = ImGui.CollapsingHeader($"##action{i}", ImGuiTreeNodeFlags.AllowOverlap);

                ImGui.SameLine();
                ImGui.TextUnformatted(action.Name);

                ImGui.SameLine(ImGui.GetContentRegionAvail().X - 60);

                if (ImGui.SmallButton("Delete"))
                {
                    settings.InputActions.RemoveAt(i);
                    ImGui.PopID();
                    i--;
                    continue;
                }

                if (open)
                {
                    DrawBindings(action);
                }

                ImGui.PopID();
            }
        }

        void DrawBindings(InputAction action)
        {
            for (int i = 0; i < action.Bindings.Count; i++)
            {
                var binding = action.Bindings[i];

                ImGui.PushID(i);

                //--------------------------------
                // Device type dropdown
                //--------------------------------

                var device = binding.DeviceType;

                if (ImGui.BeginCombo("Device", device.ToString()))
                {
                    foreach (InputDeviceType type in Enum.GetValues<InputDeviceType>())
                    {
                        bool selected = type == device;

                        if (ImGui.Selectable(type.ToString(), selected))
                            binding.DeviceType = type;

                        if (selected)
                            ImGui.SetItemDefaultFocus();
                    }

                    ImGui.EndCombo();
                }

                //--------------------------------
                // Control dropdown
                //--------------------------------

                DrawControlDropdown(binding);

                //--------------------------------
                // Scale
                //--------------------------------

                float scale = binding.Scale;
                if (ImGui.DragFloat("Scale", ref scale, 0.01f))
                    binding.Scale = scale;

                //--------------------------------
                // Clamp
                //--------------------------------

                bool clamped = binding.IsClamped;
                if (ImGui.Checkbox("Clamp", ref clamped))
                    binding.IsClamped = clamped;

                //--------------------------------
                // Remove binding
                //--------------------------------

                if (ImGui.Button("Remove"))
                {
                    action.Bindings.RemoveAt(i);
                    ImGui.PopID();
                    i--;
                    continue;
                }

                ImGui.Separator();
                ImGui.PopID();
            }

            //--------------------------------
            // Add binding
            //--------------------------------

            if (ImGui.Button("Add Binding"))
            {
                action.Bindings.Add(new InputBinding()
                {
                    DeviceType = InputDeviceType.Keyboard
                });
            }
        }

        void DrawControlDropdown(InputBinding binding)
        {
            ushort value = binding.Control;

            switch (binding.DeviceType)
            {
                case InputDeviceType.Keyboard:
                    DrawEnumDropdown<Keys>("Key", ref value);
                    break;

                case InputDeviceType.Mouse:
                    DrawEnumDropdown<MouseAxis>("Axis", ref value);
                    break;

                case InputDeviceType.Gamepad:
                    DrawEnumDropdown<GamepadStandardControl>("Gamepad", ref value);
                    break;
            }

            binding.Control = value;
        }

        void DrawEnumDropdown<T>(string label, ref ushort value) where T : Enum
        {
            T current = (T)Enum.ToObject(typeof(T), value);

            if (ImGui.BeginCombo(label, current.ToString()))
            {
                foreach (T v in Enum.GetValues(typeof(T)))
                {
                    bool selected = EqualityComparer<T>.Default.Equals(v, current);

                    if (ImGui.Selectable(v.ToString(), selected))
                        value = Convert.ToUInt16(v);

                    if (selected)
                        ImGui.SetItemDefaultFocus();
                }

                ImGui.EndCombo();
            }
        }
    }
}