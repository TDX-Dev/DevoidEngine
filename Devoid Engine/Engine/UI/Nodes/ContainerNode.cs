using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.UI.Theme;
using DevoidEngine.Engine.UI.Theme.Styleboxes;
using System.Numerics;

namespace DevoidEngine.Engine.UI.Nodes
{
    public class ContainerNode : FlexboxNode
    {
        public override string ThemeType => "Panel";


        private Vector4 _background;
        private float _borderWidth;
        private Vector4 _borderColor;
        private Vector4 _borderRadius;

        protected override void InitializeCore()
        {
            Material = UISystem.UIMaterial;
        }

        protected override void ApplyTheme()
        {
            //_background = GetColor(StyleKeys.Background);

            //_borderWidth = GetConstant<int>(StyleKeys.BorderWidth);

            //_borderRadius = GetConstant<Vector4>(StyleKeys.BorderRadius);

            //_borderColor = GetColor(StyleKeys.BorderColor);

            var style = GetStateStyleBox();

            if (style is StyleBoxFlat flat)
            {
                _background = flat.BackgroundColor;
                _borderWidth = flat.BorderWidth;
                _borderColor = flat.BorderColor;
                _borderRadius = flat.BorderRadius;
            }
            UpdateMaterial();
        }

        protected override void UpdateMaterial()
        {
            if (Material == null)
                return;

            Material.SetVector2("RECT_SIZE", Rect?.size ?? Vector2.One);

            Material.SetInt("useTexture", 0);
            //Material.SetTexture("MAT_Texture", _texture);

            Material.SetVector4("COLOR", _background);
            Material.SetFloat("BORDER_THICKNESS", _borderWidth);
            Material.SetVector4("BORDER_COLOR", _borderColor);
            Material.SetVector4("CORNER_RADIUS", _borderRadius);

        }

        protected override void UpdateCore(float dt)
        {
            base.UpdateCore(dt);
        }

        protected override void RenderCore(List<RenderItem> renderList, Matrix4x4 canvasModel, int order)
        {
            Vector2 size = VisualRect?.size ?? Vector2.One;
            Vector2 pos = VisualRect?.position ?? Vector2.One;

            // convert pivot (0–1) → pixels
            Vector2 pivotOffset = (Pivot - new Vector2(0.5f)) * size;

            Vector2 centerPos = pos + size * 0.5f;

            Matrix4x4 model =
                Matrix4x4.CreateScale(size.X, size.Y, 1f) *
                Matrix4x4.CreateTranslation(pivotOffset.X, pivotOffset.Y, 0f) *
                Matrix4x4.CreateRotationZ(Rotation) *
                Matrix4x4.CreateTranslation(centerPos.X, centerPos.Y, 0f);

            Matrix4x4 final =
                model *
                canvasModel *
                Matrix4x4.CreateTranslation(0, 0, order * UISystem.OrderEpsilon);

            var renderItem = new RenderItem()
            {
                Mesh = UISystem.QuadMesh,
                Material = Material,
                Model = final,
            };

            if (UIScissorStack.HasClip)
            {
                renderItem.useClipping = true;
                renderItem.ClipRegion = UIScissorStack.Current;
            }

            renderList.Add(renderItem);
            //DebugRenderSystem.DrawRectUI(model);
        }
    }
}