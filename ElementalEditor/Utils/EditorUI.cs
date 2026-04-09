using DevoidEngine.Engine.Core;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Utils
{
    public static class EditorUI
    {
        public static void BeginPropertyGrid(string id)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new System.Numerics.Vector2(4, 4));

            if (ImGui.BeginTable("##propgrid_" + id, 2,
                ImGuiTableFlags.SizingStretchProp |
                ImGuiTableFlags.RowBg |
                ImGuiTableFlags.BordersInnerV))
            {
                ImGui.TableSetupColumn("Label", ImGuiTableColumnFlags.WidthFixed, 150);
                ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch);
            }
        }

        public static void EndPropertyGrid()
        {
            ImGui.EndTable();
            ImGui.PopStyleVar();
        }

        public static void BeginProperty(string label)
        {
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted(label);

            ImGui.TableSetColumnIndex(1);
            ImGui.PushID(label);
        }

        public static void EndProperty()
        {
            ImGui.PopID();
        }
        public static bool PropertyFloat(ref float value, float min = float.MinValue, float max = float.MaxValue)
        {
            ImGui.SetNextItemWidth(-1);
            return ImGui.DragFloat("##value", ref value, 0.1f, min, max);
        }

        public static bool PropertyVector3(ref Vector3 value)
        {
            System.Numerics.Vector3 v = new(value.X, value.Y, value.Z);

            ImGui.SetNextItemWidth(-1);
            bool changed = ImGui.DragFloat3("##value", ref v, 0.1f);

            if (changed)
                value = new Vector3(v.X, v.Y, v.Z);

            return changed;
        }

        public static bool PropertyBool(ref bool value)
        {
            return ImGui.Checkbox("##value", ref value);
        }
        public static bool PropertyString(ref string value)
        {
            ImGui.SetNextItemWidth(-1);
            return ImGui.InputText("##value", ref value, 512);
        }

        public static bool DrawGenericField(FieldInfo field, object target)
        {
            if (field.FieldType == typeof(int))
                return DrawIntField(field, target);

            if (field.FieldType == typeof(float))
                return DrawFloatField(field, target);

            if (field.FieldType == typeof(bool))
                return DrawBoolField(field, target);

            if (field.FieldType == typeof(string))
                return DrawStringField(field, target);

            if (field.FieldType == typeof(Vector2))
                return DrawVec2Field(field, target);

            if (field.FieldType == typeof(Vector3))
                return DrawVec3Field(field, target);

            if (field.FieldType == typeof(Vector4))
                return DrawVec4Field(field, target);

            if (field.FieldType.IsEnum)
                return DrawEnumField(field, target);

            if (field.FieldType == typeof(Texture2D))
            {
                Texture2D tex = (Texture2D)field.GetValue(target);
                PropertyTexture(tex == null ? IntPtr.Zero : (IntPtr)tex.GetDeviceTexture().GetHandle());
                return false;
            }

            // fallback
            PropertyType(field.FieldType);
            return false;
        }

        public static bool DrawGenericProperty(PropertyInfo prop, object target)
        {
            if (!prop.CanRead || !prop.CanWrite)
                return false;

            if (prop.GetIndexParameters().Length > 0)
                return false;

            if (prop.PropertyType == typeof(int))
                return DrawIntProperty(prop, target);

            if (prop.PropertyType == typeof(float))
                return DrawFloatProperty(prop, target);

            if (prop.PropertyType == typeof(bool))
                return DrawBoolProperty(prop, target);

            if (prop.PropertyType == typeof(string))
                return DrawStringProperty(prop, target);

            if (prop.PropertyType == typeof(Vector2))
                return DrawVec2Property(prop, target);

            if (prop.PropertyType == typeof(Vector3))
                return DrawVec3Property(prop, target);

            if (prop.PropertyType == typeof(Vector4))
                return DrawVec4Property(prop, target);

            if (prop.PropertyType.IsEnum)
                return DrawEnumProperty(prop, target);

            PropertyType(prop.PropertyType);
            return false;
        }

        #region FIELDS
        public static bool DrawIntField(FieldInfo field, object target)
        {
            int value = (int)field.GetValue(target);

            ImGui.SetNextItemWidth(-1);
            if (ImGui.InputInt("##value", ref value))
            {
                field.SetValue(target, value);
                return true;
            }

            return false;
        }

        public static bool DrawFloatField(FieldInfo field, object target)
        {
            float value = (float)field.GetValue(target);

            if (PropertyFloat(ref value))
            {
                field.SetValue(target, value);
                return true;
            }

            return false;
        }

        public static bool DrawBoolField(FieldInfo field, object target)
        {
            bool value = (bool)field.GetValue(target);

            if (PropertyBool(ref value))
            {
                field.SetValue(target, value);
                return true;
            }

            return false;
        }

        public static bool DrawStringField(FieldInfo field, object target)
        {
            string value = (string)field.GetValue(target) ?? "";

            if (PropertyString(ref value))
            {
                field.SetValue(target, value);
                return true;
            }

            return false;
        }

        public static bool DrawVec2Field(FieldInfo field, object target)
        {
            Vector2 value = (Vector2)field.GetValue(target);

            System.Numerics.Vector2 v = new(value.X, value.Y);

            ImGui.SetNextItemWidth(-1);
            bool changed = ImGui.DragFloat2("##value", ref v, 0.1f);

            if (changed)
            {
                value = new Vector2(v.X, v.Y);
                field.SetValue(target, value);
            }

            return changed;
        }

        public static bool DrawVec3Field(FieldInfo field, object target)
        {
            Vector3 value = (Vector3)field.GetValue(target);

            if (PropertyVector3(ref value))
            {
                field.SetValue(target, value);
                return true;
            }

            return false;
        }

        public static bool DrawVec4Field(FieldInfo field, object target)
        {
            Vector4 value = (Vector4)field.GetValue(target);

            System.Numerics.Vector4 v = new(value.X, value.Y, value.Z, value.W);

            ImGui.SetNextItemWidth(-1);
            bool changed = ImGui.DragFloat4("##value", ref v, 0.1f);

            if (changed)
            {
                value = new Vector4(v.X, v.Y, v.Z, v.W);
                field.SetValue(target, value);
            }

            return changed;
        }

        public static bool DrawEnumField(FieldInfo field, object target)
        {
            string[] names = Enum.GetNames(field.FieldType);
            object value = field.GetValue(target);

            int index = Array.IndexOf(Enum.GetValues(field.FieldType), value);

            ImGui.SetNextItemWidth(-1);
            if (ImGui.Combo("##value", ref index, names, names.Length))
            {
                field.SetValue(target, Enum.GetValues(field.FieldType).GetValue(index));
                return true;
            }

            return false;
        }

        public static void PropertyTexture(IntPtr texture)
        {
            float size = ImGui.GetColumnWidth() * 0.25f;

            if (texture != IntPtr.Zero)
                ImGui.Image(texture, new Vector2(size, size));
        }

        public static void PropertyType(Type type)
        {
            ImGui.BeginDisabled();
            ImGui.Text(type.Name);
            ImGui.EndDisabled();
        }


        #endregion

        #region PROPERTIES

        public static bool DrawIntProperty(PropertyInfo prop, object target)
        {
            int value = (int)prop.GetValue(target);

            ImGui.SetNextItemWidth(-1);
            if (ImGui.InputInt("##value", ref value))
            {
                prop.SetValue(target, value);
                return true;
            }

            return false;
        }

        public static bool DrawFloatProperty(PropertyInfo prop, object target)
        {
            float value = (float)prop.GetValue(target);

            if (PropertyFloat(ref value))
            {
                prop.SetValue(target, value);
                return true;
            }

            return false;
        }

        public static bool DrawBoolProperty(PropertyInfo prop, object target)
        {
            bool value = (bool)prop.GetValue(target);

            if (PropertyBool(ref value))
            {
                prop.SetValue(target, value);
                return true;
            }

            return false;
        }

        public static bool DrawStringProperty(PropertyInfo prop, object target)
        {
            string value = (string)prop.GetValue(target) ?? "";

            if (PropertyString(ref value))
            {
                prop.SetValue(target, value);
                return true;
            }

            return false;
        }

        public static bool DrawVec2Property(PropertyInfo prop, object target)
        {
            Vector2 value = (Vector2)prop.GetValue(target);

            System.Numerics.Vector2 v = new(value.X, value.Y);

            ImGui.SetNextItemWidth(-1);
            bool changed = ImGui.DragFloat2("##value", ref v, 0.1f);

            if (changed)
            {
                value = new Vector2(v.X, v.Y);
                prop.SetValue(target, value);
            }

            return changed;
        }

        public static bool DrawVec3Property(PropertyInfo prop, object target)
        {
            Vector3 value = (Vector3)prop.GetValue(target);

            if (PropertyVector3(ref value))
            {
                prop.SetValue(target, value);
                return true;
            }

            return false;
        }

        public static bool DrawVec4Property(PropertyInfo prop, object target)
        {
            Vector4 value = (Vector4)prop.GetValue(target);

            System.Numerics.Vector4 v = new(value.X, value.Y, value.Z, value.W);

            ImGui.SetNextItemWidth(-1);
            bool changed = ImGui.DragFloat4("##value", ref v, 0.1f);

            if (changed)
            {
                value = new Vector4(v.X, v.Y, v.Z, v.W);
                prop.SetValue(target, value);
            }

            return changed;
        }

        public static bool DrawEnumProperty(PropertyInfo prop, object target)
        {
            string[] names = Enum.GetNames(prop.PropertyType);
            object value = prop.GetValue(target);

            int index = Array.IndexOf(Enum.GetValues(prop.PropertyType), value);

            ImGui.SetNextItemWidth(-1);
            if (ImGui.Combo("##value", ref index, names, names.Length))
            {
                prop.SetValue(target, Enum.GetValues(prop.PropertyType).GetValue(index));
                return true;
            }

            return false;
        }

        #endregion
    }
}
