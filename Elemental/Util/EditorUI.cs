using DevoidEngine.AssetPipeline;
using DevoidEngine.AssetPipeline.Loaders;
using DevoidEngine.Attributes;
using DevoidEngine.Core;
using DevoidGPU;
using ImGuiNET;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Elemental.Util
{
    public static class EditorUI
    {
        private static readonly MethodInfo LoadPathMethod =
            typeof(Asset)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m =>
                {
                    if (m.Name != nameof(Asset.Load) ||
                        !m.IsGenericMethodDefinition)
                        return false;

                    ParameterInfo[] parameters = m.GetParameters();

                    return parameters.Length == 2 &&
                           parameters[0].ParameterType == typeof(string) &&
                           parameters[1].ParameterType == typeof(bool);
                });
        private static readonly HashSet<MaterialInstance> openMaterialEditors = [];

        static void HandleMouseWrap()
        {
            if (!ImGui.IsMouseDragging(ImGuiMouseButton.Left))
                return;

            var io = ImGui.GetIO();
            Vector2 pos = io.MousePos;
            Vector2 display = io.DisplaySize;

            const float margin = 2f;

            if (pos.X <= margin)
                Engine.Cursor.SetCursorPosition(new Vector2(display.X - margin, pos.Y));

            else if (pos.X >= display.X - margin)
                Engine.Cursor.SetCursorPosition(new Vector2(margin, pos.Y));

            if (pos.Y <= margin)
                Engine.Cursor.SetCursorPosition(new Vector2(pos.X, display.Y - margin));

            else if (pos.Y >= display.Y - margin)
                Engine.Cursor.SetCursorPosition(new Vector2(pos.X, margin));
        }

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

            bool changed = ImGui.DragFloat("##value", ref value, 0.1f, min, max);

            if (ImGui.IsItemActive())
                HandleMouseWrap();

            return changed;
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
            {
                if (field.GetCustomAttribute<ColorField>() != null)
                    return DrawColorField(field, target);

                return DrawVec4Field(field, target);
            }

            if (field.FieldType.IsEnum)
                return DrawEnumField(field, target);

            if (field.FieldType == typeof(Texture))
            {
                Texture tex = (Texture)field.GetValue(target)!;
                PropertyTexture(tex == null ? IntPtr.Zero : (IntPtr)tex.ID);
                return false;
            }

            if (field.FieldType.IsValueType &&
                !field.FieldType.IsPrimitive &&
                !field.FieldType.IsEnum &&
                field.FieldType != typeof(Vector2) &&
                field.FieldType != typeof(Vector3) &&
                field.FieldType != typeof(Vector4))
            {
                return DrawStructField(field, target);
            }

            // fallback
            var asset = PropertyType(field.FieldType, field.GetValue(target));

            if (asset != null)
            {
                field.SetValue(target, asset);
                return true;
            }

            return false;
        }

        public static bool DrawGenericProperty(PropertyInfo prop, object target)
        {
            if (!prop.CanRead || !prop.CanWrite)
                return false;

            if (prop.GetIndexParameters().Length > 0)
                return false;

            if (prop.PropertyType == typeof(MaterialInstance))
                return DrawMaterialInstanceProperty(prop, target);

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
            {
                if (prop.GetCustomAttribute<ColorField>() != null)
                    return DrawColorProperty(prop, target);

                return DrawVec4Property(prop, target);
            }

            if (prop.PropertyType.IsEnum)
                return DrawEnumProperty(prop, target);

            if (prop.PropertyType.IsValueType &&
                !prop.PropertyType.IsPrimitive &&
                !prop.PropertyType.IsEnum &&
                prop.PropertyType != typeof(Vector2) &&
                prop.PropertyType != typeof(Vector3) &&
                prop.PropertyType != typeof(Vector4))
            {
                return DrawStructProperty(prop, target);
            }


            var asset = PropertyType(prop.PropertyType, prop.GetValue(target));

            if (asset != null)
            {
                prop.SetValue(target, asset);
                return true;
            }

            return false;
        }

        unsafe static void DrawStructRows(Type type, ref object boxed, ref bool changed)
        {
            Vector4 bg = *ImGui.GetStyleColorVec4(ImGuiCol.FrameBg);
            bg.X *= 0.9f;
            bg.Y *= 0.9f;
            bg.Z *= 0.9f;

            ImGui.PushStyleColor(ImGuiCol.TableRowBg, bg);

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                BeginProperty(field.Name);

                if (DrawGenericField(field, boxed))
                    changed = true;

                EndProperty();
            }

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.CanRead || !prop.CanWrite)
                    continue;

                BeginProperty(prop.Name);

                if (DrawGenericProperty(prop, boxed))
                    changed = true;

                EndProperty();
            }

            ImGui.PopStyleColor();
        }

        #region FIELDS

        static bool DrawStruct(Type type, ref object boxedStruct)
        {
            bool changed = false;

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                BeginProperty(field.Name);

                if (DrawGenericField(field, boxedStruct))
                    changed = true;

                EndProperty();
            }

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.CanRead || !prop.CanWrite)
                    continue;

                BeginProperty(prop.Name);

                if (DrawGenericProperty(prop, boxedStruct))
                    changed = true;

                EndProperty();
            }

            return changed;
        }

        static bool DrawStructField(FieldInfo field, object target)
        {
            object boxed = field.GetValue(target)!;

            bool changed = false;

            if (ImGui.TreeNode("##value", field.Name))
            {
                if (DrawStruct(field.FieldType, ref boxed))
                    changed = true;

                ImGui.TreePop();
            }

            if (changed)
                field.SetValue(target, boxed);

            return changed;
        }

        public static bool DrawIntField(FieldInfo field, object target)
        {
            int value = (int)field.GetValue(target)!;

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
            float value = (float)field.GetValue(target)!;

            if (PropertyFloat(ref value))
            {
                field.SetValue(target, value);
                return true;
            }

            return false;
        }

        public static bool DrawBoolField(FieldInfo field, object target)
        {
            bool value = (bool)field.GetValue(target)!;

            if (PropertyBool(ref value))
            {
                field.SetValue(target, value);
                return true;
            }

            return false;
        }

        public static bool DrawStringField(FieldInfo field, object target)
        {
            string value = (string)field.GetValue(target)! ?? "";

            if (PropertyString(ref value))
            {
                field.SetValue(target, value);
                return true;
            }

            return false;
        }

        public static bool DrawVec2Field(FieldInfo field, object target)
        {
            Vector2 value = (Vector2)field.GetValue(target)!;

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
            Vector3 value = (Vector3)field.GetValue(target)!;

            if (PropertyVector3(ref value))
            {
                field.SetValue(target, value);
                return true;
            }

            return false;
        }

        public static bool DrawVec4Field(FieldInfo field, object target)
        {
            Vector4 value = (Vector4)field.GetValue(target)!;

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

        public static bool DrawColorField(FieldInfo field, object target)
        {
            Vector4 value = (Vector4)field.GetValue(target)!;

            System.Numerics.Vector4 v = new(value.X, value.Y, value.Z, value.W);

            var attr = field.GetCustomAttribute<ColorField>();

            ImGuiColorEditFlags flags = ImGuiColorEditFlags.DisplayRGB;

            if (attr != null && attr.HDR)
                flags |= ImGuiColorEditFlags.HDR;

            ImGui.SetNextItemWidth(-1);

            bool changed = ImGui.ColorEdit4("##value", ref v, flags);

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
            object value = field.GetValue(target)!;

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

        public unsafe static object? PropertyType(Type type, object? current)
        {
            object? result = null;

            string label = current != null
                ? current.ToString()!.Split(".")[^1]
                : $"None ({type.Name})";

            ImGui.BeginDisabled();
            ImGui.Button(label, new Vector2(-1, 0));
            ImGui.EndDisabled();

            if (ImGui.BeginDragDropTarget())
            {
                // -------- OBJECT DROP --------
                var payload = ImGui.AcceptDragDropPayload("OBJECT_REF");

                if (payload.NativePtr != null)
                {
                    Console.WriteLine("NonNullRef!");
                    IntPtr ptr = *(IntPtr*)payload.Data;

                    var handle = GCHandle.FromIntPtr(ptr);
                    var dragged = handle.Target;

                    if (dragged != null && type.IsAssignableFrom(dragged.GetType()))
                        result = dragged;

                    handle.Free();
                }

                // -------- ASSET DROP --------
                var assetPayload = ImGui.AcceptDragDropPayload("ASSET_PATH");

                if (assetPayload.NativePtr != null)
                {
                    string path = Encoding.UTF8.GetString(
                        (byte*)assetPayload.Data,
                        assetPayload.DataSize
                    );

                    if (SupportsAssetType(type))
                    {
                        MethodInfo loadMethod =
                            LoadPathMethod.MakeGenericMethod(type);

                        result = loadMethod.Invoke(null, [path, true]);
                    }
                }

                ImGui.EndDragDropTarget();
            }

            return result;
        }

        static bool SupportsAssetType(Type type)
        {
            return AssetLoaderRegistry.HasLoader(type);
        }


        #endregion

        #region PROPERTIES

        static bool DrawStructProperty(PropertyInfo prop, object target)
        {
            object boxed = prop.GetValue(target)!;
            bool changed = false;

            BeginProperty(prop.Name);

            bool open = ImGui.TreeNodeEx("##struct", ImGuiTreeNodeFlags.SpanFullWidth);

            EndProperty();

            if (open)
            {
                DrawStructRows(prop.PropertyType, ref boxed, ref changed);
                ImGui.TreePop();
            }

            if (changed)
                prop.SetValue(target, boxed);

            return changed;
        }

        public static bool DrawIntProperty(PropertyInfo prop, object target)
        {
            int value = (int)prop.GetValue(target)!;

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
            float value = (float)prop.GetValue(target)!;

            if (PropertyFloat(ref value))
            {
                prop.SetValue(target, value);
                return true;
            }

            return false;
        }

        public static bool DrawBoolProperty(PropertyInfo prop, object target)
        {
            bool value = (bool)prop.GetValue(target)!;

            if (PropertyBool(ref value))
            {
                prop.SetValue(target, value);
                return true;
            }

            return false;
        }

        public static bool DrawStringProperty(PropertyInfo prop, object target)
        {
            string value = (string)prop.GetValue(target)! ?? "";

            if (PropertyString(ref value))
            {
                prop.SetValue(target, value);
                return true;
            }

            return false;
        }

        public static bool DrawVec2Property(PropertyInfo prop, object target)
        {
            Vector2 value = (Vector2)prop.GetValue(target)!;

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
            Vector3 value = (Vector3)prop.GetValue(target)!;

            if (PropertyVector3(ref value))
            {
                prop.SetValue(target, value);
                return true;
            }

            return false;
        }

        public static bool DrawVec4Property(PropertyInfo prop, object target)
        {
            Vector4 value = (Vector4)prop.GetValue(target)!;

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
            object value = prop.GetValue(target)!;

            int index = Array.IndexOf(Enum.GetValues(prop.PropertyType), value);

            ImGui.SetNextItemWidth(-1);
            if (ImGui.Combo("##value", ref index, names, names.Length))
            {
                prop.SetValue(target, Enum.GetValues(prop.PropertyType).GetValue(index));
                return true;
            }

            return false;
        }

        public static bool DrawColorProperty(PropertyInfo prop, object target)
        {
            Vector4 value = (Vector4)prop.GetValue(target)!;

            System.Numerics.Vector4 v = new(value.X, value.Y, value.Z, value.W);

            var attr = prop.GetCustomAttribute<ColorField>();

            ImGuiColorEditFlags flags = ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.DisplayRGB;// | ImGuiColorEditFlags.DisplayHSV;

            if (attr != null && attr.HDR)
                flags |= ImGuiColorEditFlags.HDR;

            ImGui.SetNextItemWidth(-1);

            bool changed = ImGui.ColorEdit4("##value", ref v, flags);

            if (changed)
            {
                value = new Vector4(v.X, v.Y, v.Z, v.W);
                prop.SetValue(target, value);
            }

            return changed;
        }

        public static bool DrawMaterialInstanceProperty(PropertyInfo property, object target)
        {
            if (property.GetValue(target) is not MaterialInstance material)
            {
                ImGui.BeginDisabled();

                ImGui.Button(
                    $"None ({nameof(MaterialInstance)})",
                    new Vector2(-1, 0));

                ImGui.EndDisabled();

                return false;
            }

            ImGui.SetNextItemWidth(-1);
            ImGui.Button("Material", new Vector2(-1, 0));

            if (ImGui.IsItemHovered() &&
                ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                openMaterialEditors.Add(material);
            }

            return false;
        }

        #endregion

        public static void DrawMaterialInstanceEditors()
        {
            foreach (var material in openMaterialEditors)
            {
                DrawMaterialInstanceEditorWindow(material);
            }
        }

        private static void DrawMaterialInstanceEditorWindow(MaterialInstance material)
        {
            bool open = true;

            string title =
                $"Material Instance###{material.GetHashCode()}";

            if (ImGui.Begin(title, ref open))
            {
                ImGui.TextUnformatted("Material Instance");
                ImGui.Separator();

                ImGui.TextUnformatted($"Shader: {material.BaseMaterial.Shader.Name}");

                ImGui.Spacing();

                DrawMaterialInstanceEditor(material);
            }

            ImGui.End();

            if (!open)
                openMaterialEditors.Remove(material);
        }

        private static bool DrawMaterialInstanceEditor(MaterialInstance material)
        {
            bool changed = false;

            if (ImGui.BeginTable(
                "MaterialProperties",
                2,
                ImGuiTableFlags.SizingStretchProp |
                ImGuiTableFlags.BordersInnerV))
            {
                ImGui.TableSetupColumn(
                    "Property",
                    ImGuiTableColumnFlags.WidthFixed,
                    140);

                ImGui.TableSetupColumn(
                    "Value",
                    ImGuiTableColumnFlags.WidthStretch);

                foreach (var variable in material.BaseMaterial.GetVariables())
                {
                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.TextUnformatted(variable.Key);

                    ImGui.TableSetColumnIndex(1);

                    if (DrawMaterialVariable(
                        material,
                        variable.Key,
                        variable.Value))
                    {
                        changed = true;
                    }
                }

                foreach (var texture in material.BaseMaterial.GetTextureBindings())
                {
                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.TextUnformatted(texture.Key);

                    ImGui.TableSetColumnIndex(1);

                    if (DrawMaterialTexture(material, texture.Key))
                        changed = true;
                }

                ImGui.EndTable();
            }

            return changed;
        }

        private static bool DrawMaterialVariable(
            MaterialInstance material,
            string name,
            ShaderVariableInfo info)
        {
            ReadOnlySpan<byte> raw =
                material.GetRawValue(name);

            switch (info.Type)
            {
                case ShaderVariableType.Float:
                    {
                        float value = MemoryMarshal.Read<float>(raw);

                        if (ImGui.DragFloat(
                            $"##{name}",
                            ref value,
                            0.01f))
                        {
                            material.SetFloat(name, value);
                            return true;
                        }

                        break;
                    }

                case ShaderVariableType.Int:
                    {
                        int value = MemoryMarshal.Read<int>(raw);

                        if (ImGui.DragInt(
                            $"##{name}",
                            ref value))
                        {
                            material.SetInt(name, value);
                            return true;
                        }

                        break;
                    }

                case ShaderVariableType.Vector2:
                    {
                        Vector2 value = MemoryMarshal.Read<Vector2>(raw);

                        if (ImGui.DragFloat2(
                            $"##{name}",
                            ref value,
                            0.01f))
                        {
                            material.SetVector2(name, value);
                            return true;
                        }

                        break;
                    }

                case ShaderVariableType.Vector3:
                    {
                        Vector3 value = MemoryMarshal.Read<Vector3>(raw);

                        if (ImGui.DragFloat3(
                            $"##{name}",
                            ref value,
                            0.01f))
                        {
                            material.SetVector3(name, value);
                            return true;
                        }

                        break;
                    }

                case ShaderVariableType.Vector4:
                    {
                        Vector4 value = MemoryMarshal.Read<Vector4>(raw);

                        if (ImGui.DragFloat4(
                            $"##{name}",
                            ref value,
                            0.01f))
                        {
                            material.SetVector4(name, value);
                            return true;
                        }

                        break;
                    }

                case ShaderVariableType.Matrix4x4:
                    {
                        ImGui.TextUnformatted(
                            MemoryMarshal.Read<Matrix4x4>(raw).ToString());

                        break;
                    }

                default:
                    ImGui.TextUnformatted($"Unsupported: {info.Type}");
                    break;
            }

            return false;
        }

        private static bool DrawMaterialTexture(MaterialInstance material, string name)
        {
            Texture? texture;

            if (material.IsTextureOverridden(name))
                texture = material.GetTextureOverride(name);
            else
                texture = material.BaseMaterial.GetDefaultTexture(name);

            bool changed = false;

            if (texture != null)
            {
                ImGui.TextUnformatted(texture.Guid.ToString());
            }
            else
            {
                ImGui.TextUnformatted("None");
            }

            ImGui.SameLine();

            if (ImGui.SmallButton($"Clear##{name}"))
            {
                material.ClearTextureOverride(name);
                changed = true;
            }

            return changed;
        }
    }
}
