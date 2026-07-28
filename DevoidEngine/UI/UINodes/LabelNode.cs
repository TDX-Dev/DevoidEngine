using DevoidEngine.Core;
using DevoidEngine.UI.Text;
using DevoidEngine.UI.Theme;
using DevoidEngine.Util;
using DevoidGPU;
using System.Numerics;

namespace DevoidEngine.UI.UINodes
{
    public class LabelNode : UINode
    {
        public override string ThemeType => "Label";

        public TextOverflow Overflow
        {
            get => _overflow;
            set
            {
                if (_overflow == value)
                    return;

                _overflow = value;
                _layoutDirty = true;
            }
        }

        public TextHorizontalAlignment HorizontalAlignment
        {
            get => _horizontalAlignment;
            set
            {
                if (_horizontalAlignment == value)
                    return;

                _horizontalAlignment = value;
                _layoutDirty = true;
            }
        }

        public TextVerticalAlignment VerticalAlignment
        {
            get => _verticalAlignment;
            set
            {
                if (_verticalAlignment == value)
                    return;

                _verticalAlignment = value;
                _layoutDirty = true;
            }
        }

        private TextOverflow _overflow = TextOverflow.None;

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

        private readonly Mesh _mesh = new(ResourceUsage.Dynamic);
        private readonly TextLayoutResult _measureLayout = new();
        private readonly TextLayoutResult _renderLayout = new();

        private Vector2 _renderConstraint;
        private bool _layoutDirty = true;
        private bool _meshDirty = true;

        private TextHorizontalAlignment _horizontalAlignment = TextHorizontalAlignment.Left;
        private TextVerticalAlignment _verticalAlignment = TextVerticalAlignment.Top;

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
            Vector2 constraint = Overflow == TextOverflow.None
                ? Vector2.Zero
                : availableSize;

            BuildLayout(
                _measureLayout,
                constraint);

            return new Vector2(
                _measureLayout.Width,
                _measureLayout.Height);
        }

        protected override void ArrangeCore(Rect finalRect)
        {
            Vector2 constraint = Overflow == TextOverflow.None
                ? Vector2.Zero
                : finalRect.Size;

            if (!_layoutDirty &&
                _renderConstraint == constraint)
                return;

            BuildLayout(
                _renderLayout,
                constraint);

            _renderConstraint = constraint;

            _layoutDirty = false;
            _meshDirty = true;
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

            if (Material == null || string.IsNullOrEmpty(Text))
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

        private void BuildLayout(
            TextLayoutResult layout,
            Vector2 constraint)
        {
            TextLayoutSettings settings = new()
            {
                FontSize = FontSize,

                Overflow = Overflow,

                MaxWidth = Overflow == TextOverflow.None
                    ? 0
                    : constraint.X,

                MaxHeight = Overflow == TextOverflow.None
                    ? 0
                    : constraint.Y,

                HorizontalAlignment = TextHorizontalAlignment.Left,
                VerticalAlignment = TextVerticalAlignment.Top,
            };

            TextLayout.Layout(
                Font,
                Text,
                settings,
                layout);
        }

        private void RebuildMesh()
        {
            if (string.IsNullOrEmpty(Text))
            {
                _meshDirty = false;
                return;
            }

            TextMeshBuilder.Build(
                Font,
                _renderLayout,
                FontSize,
                _mesh);

            Material?.SetTexture(
                "MAT_Texture",
                Font.FontAtlasTexture);

            _meshDirty = false;

            Console.WriteLine("Rebuilt Mesh");
        }
    }
}
