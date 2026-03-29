using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.UI.Theme;
using System.Numerics;

namespace DevoidEngine.Engine.UI.Nodes
{
    public class ContainerNode : FlexboxNode
    {
        public override string ThemeType => "Panel";


        public float BorderThickness = 0f;
        public Vector4 BorderColor = new Vector4(0, 0, 0, 1);

        private Vector4 _color = new Vector4(1, 1, 1, 1);
        private float _opacity = 1f;
        private Vector4 _borderRadius = new Vector4(0, 0, 0, 0);

        protected override void InitializeCore()
        {
            Material = UISystem.UIMaterial;
            UpdateMaterial();
        }

        protected override void ApplyTheme()
        {
            var theme = GetTheme();

            if (theme.HasColor(StyleKeys.Background, ThemeType))
                Color = theme.GetColor(StyleKeys.Background, ThemeType);

            if (theme.HasConstant(StyleKeys.BorderWidth, ThemeType))
                BorderThickness = theme.GetConstant(StyleKeys.BorderWidth, ThemeType);
        }

        private void UpdateMaterial()
        {
            if (Material == null)
                return;

            Material.SetInt("useTexture", _texture != null ? 1 : 0);
            Material.SetTexture("MAT_Texture", _texture);

            Vector4 final = _color;
            final.W *= _opacity;

            Material.SetVector4("COLOR", final);
            Material.SetVector2("RECT_SIZE", Rect?.size ?? Vector2.One);
            Material.SetVector4("CORNER_RADIUS", _borderRadius);
            Material.SetVector4("BORDER_COLOR", BorderColor);
            Material.SetFloat("BORDER_THICKNESS", BorderThickness);

        }

        protected override void RenderCore(List<RenderItem> renderList, Matrix4x4 canvasModel, int order)
        {
            Vector2 size = Rect.size;
            Vector2 pos = Rect.position;

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

            renderList.Add(new RenderItem()
            {
                Mesh = UISystem.QuadMesh,
                Material = Material,
                Model = final
            });
            //DebugRenderSystem.DrawRectUI(model);
        }

        //protected override void UpdateCore(float deltaTime)
        //{

        //}

        //public override void OnMouseDown()
        //{
        //    OnMouse
        //}

        //public override void OnDrag(Vector2 mouse, Vector2 delta)
        //{
        //    Offset += delta;
        //}
    }
}