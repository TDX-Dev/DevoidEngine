using DevoidEngine.Core;
using DevoidEngine.InputSystem;
using DevoidEngine.InputSystem.InputDevices;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elemental.ProjectSettings
{
    public class InputSettingsProvider : IProjectSettingsProvider
    {
        public string Category => "Input System";
        public string Name => "Input";

        string newActionName = "";

        public void Draw()
        {
            var settings = Engine.Instance.ProjectSystem.Settings;


            ImGui.InputText("New Action", ref newActionName, 64);

            ImGui.SameLine();

            if (ImGui.Button("Add Action") && !string.IsNullOrWhiteSpace(newActionName))
            {
                settings.Input.Actions.Add(new InputAction
                {
                    Name = newActionName
                });

                newActionName = "";
            }

            ImGui.Separator();

            for (int i = 0; i < settings.Input.Actions.Count; i++)
            {
                var action = settings.Input.Actions[i];

                ImGui.PushID(i);

                bool open = ImGui.CollapsingHeader($"##action{i}", ImGuiTreeNodeFlags.AllowOverlap);

                ImGui.SameLine();
                ImGui.TextUnformatted(action.Name);

                ImGui.SameLine(ImGui.GetContentRegionAvail().X - 60);

                if (ImGui.SmallButton("Delete"))
                {
                    settings.Input.Actions.RemoveAt(i);
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


                DrawControlDropdown(binding);


                float scale = binding.Scale;
                if (ImGui.DragFloat("Scale", ref scale, 0.01f))
                    binding.Scale = scale;

                bool clamped = binding.IsClamped;
                if (ImGui.Checkbox("Clamp", ref clamped))
                    binding.IsClamped = clamped;

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
