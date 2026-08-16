using DevoidEngine.AssetPipeline;
using DevoidEngine.Assets;
using DevoidEngine.Core;
using DevoidEngine.Rendering;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Elemental
{
    public static class EditorIcons
    {
        private static readonly Dictionary<string, Texture> _iconTextures = [];
        private static readonly Dictionary<string, IntPtr> _iconHandles = [];

        // Convenience properties for common icons
        public static IntPtr GameObject { get; private set; }
        public static IntPtr Folder { get; private set; }
        public static IntPtr Camera { get; private set; }
        public static IntPtr Mesh { get; private set; }
        public static IntPtr Light { get; private set; }

        public static void Initialize()
        {
            GameObject = Load("box", "Assets/Editor/Icons/box.png");
            Folder = Load("folder", "Assets/Editor/Icons/folder.png");
            Camera = Load("camera", "Assets/Editor/Icons/camera.png");
            Mesh = Load("mesh", "Assets/Editor/Icons/mesh.png");
            Light = Load("light", "Assets/Editor/Icons/sun.png");
        }

        public static IntPtr Load(string key, string path)
        {
            if (_iconHandles.TryGetValue(key, out var handle))
                return handle;

            var texture = Asset.Load<Texture>(path);
            if (texture == null) return IntPtr.Zero;

            _iconTextures[key] = texture;

            // Adapt this line to your RHI's method for retrieving ImGui-compatible texture IDs
            IntPtr imguiId = (IntPtr)texture.ID;
            _iconHandles[key] = imguiId;

            return imguiId;
        }

        public static IntPtr Get(string key)
        {
            return _iconHandles.TryGetValue(key, out var handle) ? handle : IntPtr.Zero;
        }

        public static void DrawIcon(IntPtr handle, float size = 16f)
        {
            if (handle == IntPtr.Zero) return;
            ImGui.Image(handle, new Vector2(size, size));
        }

        public static void Shutdown()
        {
            _iconTextures.Clear();
            _iconHandles.Clear();
        }
    }
}