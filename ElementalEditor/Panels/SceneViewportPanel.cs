using DevoidEngine.Engine.AssetPipeline;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.GizmoSystem;
using ElementalEditor.Scripting;
using ElementalEditor.Utils;
using ImGuiNET;
using System.Numerics;
using System.Text;

namespace ElementalEditor.Panels
{
    public class SceneViewportPanel : IEditorPanel
    {
        private Vector2 lastViewportSize;

        public Vector2 ViewportSize;
        public Vector2 GameMousePosition;
        public bool MouseInsideViewport;

        Vector2 gizmoPopupPosition;
        Vector2 cameraPopupPosition;

        const float ToolbarHeight = 36f;

        public void Draw(EditorContext context)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
            if (!ImGui.Begin("Scene"))
            {
                ImGui.PopStyleVar();
                ImGui.End();
                return;
            }

            if (context.EditorCamera == null)
            {
                ImGui.PopStyleVar();
                ImGui.End(); 
                return;
            }


            Vector2 panelSize = ImGui.GetContentRegionAvail();
            //panelSize.Y -= ToolbarHeight;

            ViewportSize = panelSize;

            int width = (int)ViewportSize.X;
            int height = (int)ViewportSize.Y;

            if (width > 0 && height > 0)
            {
                if (lastViewportSize.X != width || lastViewportSize.Y != height)
                {
                    context.EditorCamera.SetViewportSize(width, height);
                    //context.Scene.ResizeCameras(width, height);
                    lastViewportSize = new Vector2(width, height);
                    Screen.Size = new Vector2(width, height);
                }
            }

            var texture = (Texture2D)context.EditorCamera.Camera.RenderTarget.GetRenderTexture(0);

            Vector2 contentMin = ImGui.GetCursorScreenPos();
            Vector2 contentMax = contentMin + ViewportSize;

            var drawList = ImGui.GetWindowDrawList();

            drawList.AddRectFilled(
                contentMin,
                contentMax,
                ImGui.GetColorU32(new Vector4(0, 0, 0, 1))
            );

            ImGui.Image(
                texture.GetDeviceTexture().GetHandle(),
                ViewportSize
            );

            if (ImGui.BeginDragDropTarget())
            {
                var payload = ImGui.AcceptDragDropPayload("ASSET_PATH");

                unsafe
                {
                    if (payload.NativePtr != null)
                    {
                        unsafe
                        {
                            string path = Encoding.UTF8.GetString(
                                (byte*)payload.Data,
                                payload.DataSize
                            );

                            HandleAssetDrop(context, path);
                        }
                    }
                }

                ImGui.EndDragDropTarget();
            }

            DrawViewportTools(contentMin, context);

            Vector2 mousePos = ImGui.GetMousePos();

            Vector2 viewportMin = contentMin;
            Vector2 viewportMax = contentMin + ViewportSize;

            MouseInsideViewport =
                mousePos.X >= viewportMin.X &&
                mousePos.Y >= viewportMin.Y &&
                mousePos.X <= viewportMax.X &&
                mousePos.Y <= viewportMax.Y;

            if (MouseInsideViewport)
            {
                GameMousePosition = mousePos - viewportMin;
            }

            context.ViewportFocused =
                MouseInsideViewport &&
                ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows);

            ImGui.PopStyleVar();
            ImGui.End();
        }

        void HandleAssetDrop(EditorContext context, string relativePath)
        {
            string ext = Path.GetExtension(relativePath).ToLower();

            if (ext == ".gltf" || ext == ".glb")
            {
                var model = Asset.Load<Model>(relativePath);

                if (model != null)
                {
                    model.Instantiate(context.Scene);
                }
            }

            if (ext == ".scene")
            {
                var scene = Asset.Load<Scene>(relativePath, false);
                context.Scene = scene;
                SceneManager.LoadScene(scene);
            }
        }

        void DrawViewportTools(Vector2 viewportMin, EditorContext context)
        {
            Vector2 pos = viewportMin + new Vector2(10, 10);

            ImGui.SetCursorScreenPos(pos);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(8, 6));

            ImGui.BeginChild(
                "ViewportToolsOverlay",
                new Vector2(0, 0),
                ImGuiChildFlags.Borders |
                ImGuiChildFlags.AlwaysAutoResize |
                ImGuiChildFlags.AutoResizeX |
                ImGuiChildFlags.AutoResizeY
            );


            if (ImGui.Button(BootstrapIconFont.Sliders))
            {
                Vector2 buttonMin = ImGui.GetItemRectMin();
                Vector2 buttonMax = ImGui.GetItemRectMax();

                gizmoPopupPosition = new Vector2(buttonMin.X, buttonMax.Y);
                ImGui.OpenPopup("GizmoSettings");
            }

            ImGui.SameLine();
            if (ImGui.Button(BootstrapIconFont.CameraFill))
            {
                Vector2 buttonMin = ImGui.GetItemRectMin();
                Vector2 buttonMax = ImGui.GetItemRectMax();

                cameraPopupPosition = new Vector2(buttonMin.X, buttonMax.Y);
                ImGui.OpenPopup("CameraSettings");
            }

            ImGui.SameLine();
            if (ImGui.Button(BootstrapIconFont.Compass))
            {
                if (ScriptCompiler.Compile(out string errors))
                {
                    ScriptAssemblyLoader.Load();
                }

                Console.WriteLine(errors);
            }

            DrawGizmoPopup(gizmoPopupPosition);
            DrawCameraPopup(context, cameraPopupPosition);

            ImGui.EndChild();

            ImGui.PopStyleVar();
        }

        void DrawCameraPopup(EditorContext context, Vector2 position)
        {
            ImGui.SetNextWindowPos(position, ImGuiCond.Appearing);
            if (!ImGui.BeginPopup("CameraSettings"))
                return;

            var cam = context.EditorCamera;

            float fov = cam.Fov;
            if (ImGui.SliderFloat("FOV", ref fov, 30f, 120f))
            {
                cam.Fov = fov;
                cam.UpdateView();
            }

            float speed = cam.MoveSpeed;
            if (ImGui.SliderFloat("Move Speed", ref speed, 1f, 100f))
                cam.MoveSpeed = speed;

            float sensitivity = cam.MouseSensitivity;
            if (ImGui.SliderFloat("Mouse Sensitivity", ref sensitivity, 0.01f, 1f))
                cam.MouseSensitivity = sensitivity;

            ImGui.Separator();

            ImGui.Text("Position");

            Vector3 pos = cam.Position;
            if (ImGui.DragFloat3("##pos", ref pos))
            {
                cam.Position = pos;
                cam.UpdateView();
            }

            ImGui.EndPopup();
        }

        void DrawGizmoPopup(Vector2 position)
        {
            ImGui.SetNextWindowPos(position, ImGuiCond.Appearing);
            if (!ImGui.BeginPopup("GizmoSettings"))
                return;

            bool enabled = Gizmos.Enabled;
            if (ImGui.Checkbox("Enable Gizmos", ref enabled))
                Gizmos.Enabled = enabled;

            ImGui.Separator();

            foreach (GizmoCategory cat in Enum.GetValues<GizmoCategory>())
            {
                if (cat == GizmoCategory.None || cat == GizmoCategory.All)
                    continue;

                bool active = (Gizmos.EnabledCategories & cat) != 0;

                if (ImGui.Checkbox(cat.ToString(), ref active))
                {
                    if (active)
                        Gizmos.EnabledCategories |= cat;
                    else
                        Gizmos.EnabledCategories &= ~cat;
                }
            }

            ImGui.EndPopup();
        }
    }
}