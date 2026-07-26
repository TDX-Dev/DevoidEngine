using DevoidEngine.Core;
using DevoidEngine.UI.Theme.Styleboxes;
using DevoidEngine.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.UI.UINodes
{
    public class ContainerNode : FlexboxNode
    {
        public override string ThemeType => "Panel";


        private Vector4 _background;
        private float _borderWidth;
        private Vector4 _borderColor;
        private Vector4 _borderRadius;
        private bool _useTexture;
        private Texture? _texture;

        protected override void InitializeCore()
        {
            Material = Engine.UISystem.DefaultBoxMaterial;
        }

        public override void Add(UINode child)
        {
            base.Add(child);
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
            else if (style is StyleBoxTexture stex)
            {
                _useTexture = stex.Texture != null;
                _texture = stex.Texture;
            }

            UpdateMaterial();
        }

        protected override void UpdateMaterial()
        {
            if (Material == null)
                return;

            Material.SetVector2("RECT_SIZE", Rect.Size);

            Material.SetInt("useTexture", _useTexture ? 1 : 0);
            if (_useTexture && _texture != null)
            {
                Material.SetTexture("MAT_Texture", _texture);
            }

            Material.SetVector4("COLOR", _background);
            Material.SetFloat("BORDER_THICKNESS", _borderWidth);
            Material.SetVector4("BORDER_COLOR", _borderColor);
            Material.SetVector4("CORNER_RADIUS", _borderRadius);

        }

        protected override void UpdateCore(float dt)
        {

            base.UpdateCore(dt);
        }

        protected override void PreRender(UIDrawList drawList, int order)
        {
            if (Material == null) return;

            Material.SetVector2("RECT_SIZE", Rect.Size);

            Vector2 size = VisualRect.Size;
            Vector2 pos = VisualRect.Position;

            Vector2 pivotOffset = (Pivot - new Vector2(0.5f)) * size;
            Vector2 centerPos = pos + size * 0.5f;

            Rect renderQuadRect = new(centerPos, size);

            drawList.AddQuad(
                renderQuadRect,
                Material,
                order,
                Rotation,
                pivotOffset
            );

        }
        //{
        //    if (Material == null) return;

        //    Material.SetVector2("RECT_SIZE", Rect.Size);

        //    Vector2 size = VisualRect.Size;
        //    Vector2 pos = VisualRect.Position;

        //    Vector2 pivotOffset = (Pivot - new Vector2(0.5f)) * size;
        //    Vector2 centerPos = pos + size * 0.5f;

        //    Matrix4x4 model =
        //        Matrix4x4.CreateScale(size.X, size.Y, 1f) *
        //        Matrix4x4.CreateTranslation(pivotOffset.X, pivotOffset.Y, 0f) *
        //        Matrix4x4.CreateRotationZ(Rotation) *
        //        Matrix4x4.CreateTranslation(centerPos.X, centerPos.Y, 0f);

        //    Matrix4x4 final =
        //        model *
        //        Matrix4x4.CreateTranslation(0, 0, order * UISystem.OrderEpsilon) *
        //        canvasModel;

        //    var renderItem = new RenderItem()
        //    {
        //        Mesh = UISystem.QuadMesh,
        //        Material = Material,
        //        Model = final,
        //    };

        //    if (UIScissorStack.HasClip)
        //    {
        //        renderItem.useClipping = true;
        //        renderItem.ClipRegion = UIScissorStack.Current;
        //    }

        //    renderList.Add(renderItem);
        //}
    }
}
