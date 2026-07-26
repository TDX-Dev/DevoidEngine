using DevoidEngine.Core;
using DevoidEngine.UI.Text;
using DevoidEngine.UI.Theme;
using DevoidEngine.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.UI.UINodes
{
    public class LabelNode : UINode
    {
        public override string ThemeType => "Label";

        public string Text
        {
            get => _text;
            set
            {
                if (_text == value)
                    return;

                _text = value;
                _layoutDirty = true;
            }
        }

        public Font Font
        {
            get => _font;
            set
            {
                if (_font == value)
                    return;

                _font = value;
                _layoutDirty = true;

                UpdateMaterial();
            }
        }

        public float FontSize
        {
            get => _fontSize;
            set
            {
                if (_fontSize == value)
                    return;

                _fontSize = value;
                _layoutDirty = true;
            }
        }

        private string _text = string.Empty;
        private Font _font = null!;
        private float _fontSize = 16;

        private Mesh? _mesh;
        private TextLayoutResult? _layout;
        private Vector2 _layoutSize;
        private bool _layoutDirty = true;
        private bool _meshDirty = true;

        //private MaterialInstance? _material;

        private Vector4 _fontColor;

        protected override void ApplyTheme()
        {
            _fontColor = GetColor(StyleKeys.FontColor);

            UpdateMaterial();
        }

        protected override void UpdateMaterial()
        {
            if (Material == null)
                return;

            Material.SetTexture("MAT_Texture", Font.FontAtlasTexture);
            Material.SetVector4("COLOR", _fontColor);
            Material.SetInt("SDF_PIXEL_RANGE", Font.SDFPixelRange);
        }

        protected override void InitializeCore()
        {
            Material = Engine.UISystem.TextSDFMaterial;
        }

        protected override Vector2 MeasureCore(Vector2 availableSize)
        {
            UpdateLayout(availableSize);

            return new Vector2(_layout!.Width, _layout.Height);
        }

        protected override void ArrangeCore(Rect finalRect)
        {
            UpdateLayout(finalRect.Size);
        }

        protected override void UpdateCore(float dt)
        {
            if (_meshDirty)
                RebuildMesh();
        }

        protected override void PreRender(
            UIDrawList drawList,
            int order)
        {
            if (_mesh == null)
                return;

            if (Material == null)
                return;

            //Vector2 size = VisualRect.Size;
            //Vector2 pivotOffset = (Pivot - new Vector2(0.5f)) * size;
            Vector2 pivotOffset = Vector2.Zero;

            drawList.AddText(
                VisualRect,
                _mesh,
                Material,
                order,
                Rotation,
                pivotOffset
            );
        }
        protected override void PostRender(UIDrawList drawList)
        {

        }

        private void UpdateLayout(Vector2 maxSize)
        {
            if (!_layoutDirty &&
                _layoutSize == maxSize)
                return;

            TextLayoutSettings settings = new()
            {
                FontSize = FontSize,
                MaxWidth = maxSize.X,
                MaxHeight = maxSize.Y,
                HorizontalAlignment = TextHorizontalAlignment.Left,
                VerticalAlignment = TextVerticalAlignment.Top,
            };

            _layout = TextLayout.Layout(Font, Text, settings);

            _layoutSize = maxSize;

            _layoutDirty = false;
            _meshDirty = true;
        }

        private void RebuildMesh()
        {
            if (_layout == null)
                return;

            _mesh = TextMeshBuilder.Build(
                Font,
                _layout,
                FontSize);

            Material?.SetTexture(
                "MAT_Texture",
                Font.FontAtlasTexture);

            _meshDirty = false;
        }
    }
}
