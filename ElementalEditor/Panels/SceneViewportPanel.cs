using DevoidEngine.Engine.AssetPipeline;
using DevoidEngine.Engine.Core;
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
                scene.Play(true);
            }
        }
    }
}