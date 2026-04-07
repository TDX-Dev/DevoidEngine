using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.UI.Text;
using DevoidEngine.Engine.UI.Theme;
using System.Numerics;

namespace DevoidEngine.Engine.UI.Nodes
{
    public class LabelNode : UINode
    {
        public TextLayoutOptions LayoutOptions = TextLayoutOptions.Default;

        public override string ThemeType => "Label";

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                _meshDirty = true;
            }
        }

        public FontInternal? Font;
        public float Scale = 1f;

        private Mesh? _mesh;
        private string _text;
        private int _fontSize;
        private Vector4 _color = new Vector4(1, 1, 1, 1);
        bool _meshDirty = true;

        private float _lastWidthConstraint = float.PositiveInfinity;

        public LabelNode(string text, float scale = 16f)
        {
            Font = GetFont(StyleKeys.Font);
            Scale = scale;
            _text = text;
            _meshDirty = true;

            Layout.FlexGrowMain = 0;
            Layout.FlexGrowCross = 0;
        }

        public LabelNode(string text, FontInternal font, float scale = 16f)
        {
            Font = font;
            Scale = scale;
            _text = text;
            _meshDirty = true;

            Layout.FlexGrowMain = 0;
            Layout.FlexGrowCross = 0;
        }

        protected override void ApplyTheme()
        {
            _color = GetColor(StyleKeys.FontColor);
            _fontSize = GetFontSize(StyleKeys.FontSize);
        }

        private void UpdateMesh(float widthConstraint)
        {
            if (Font == null)
                return;
            var opts = LayoutOptions;

            //if (!float.IsInfinity(widthConstraint))
            //    opts.MaxWidth = widthConstraint;

            var newMesh = TextMeshGenerator.Generate(
                Font,
                _text,
                Font.GetScaleForFontSize(Scale),
                opts
            );

            var oldMesh = _mesh;
            _mesh = newMesh;

            if (oldMesh != null)
                oldMesh.Dispose();
        }

        protected override Vector2 MeasureCore(Vector2 availableSize)
        {
            if (Font == null)
                return Vector2.Zero;

            var opts = LayoutOptions;

            float widthConstraint = opts.MaxWidth;

            if (float.IsInfinity(widthConstraint))
                widthConstraint = availableSize.X;

            opts.MaxWidth = widthConstraint;

            Vector2 textSize = TextMeshGenerator.Measure(
                Font,
                _text,
                Font.GetScaleForFontSize(Scale),
                opts
            );

            textSize.Y = Math.Max(
                (Font.Ascender - Font.Descender) * Font.GetScaleForFontSize(Scale),
                textSize.Y
            );

            return textSize;
        }

        protected override void ArrangeCore(UITransform finalRect)
        {
            Rect = finalRect;

            if (Font == null || string.IsNullOrEmpty(Text))
                return;

            float widthConstraint = finalRect.Size.X;

            //if (widthConstraint <= 0)
            //    widthConstraint = float.PositiveInfinity;

            if (_meshDirty || _lastWidthConstraint != widthConstraint)
            {
                _meshDirty = false;
                _lastWidthConstraint = widthConstraint;
                UpdateMesh(widthConstraint);
            }
        }

        protected override void RenderCore(List<RenderItem> renderList, Matrix4x4 canvasModel, int order)
        {
            if (Font == null || string.IsNullOrEmpty(Text) || _mesh == null || Material == null)
                return;

            Vector2 pos = VisualRect!.Position;

            pos.X = MathF.Round(pos.X);
            pos.Y = MathF.Round(pos.Y);

            Matrix4x4 local =
                UISystem.BuildTranslationModel(new UITransform(pos, VisualRect!.Size)) *
                Matrix4x4.CreateTranslation(Pivot.X, Pivot.Y, 0) *
                Matrix4x4.CreateRotationX(Rotation);

            Matrix4x4 final =
                local *
                Matrix4x4.CreateTranslation(0, 0, order * UISystem.OrderEpsilon) *
                canvasModel;

            RenderItem renderItem = new RenderItem()
            {
                Mesh = _mesh,
                Material = Material,
                Model = final,
            };

            if (UIScissorStack.HasClip)
            {
                renderItem.useClipping = true;
                renderItem.ClipRegion = UIScissorStack.Current;
            }

            renderList.Add(renderItem);
        }

        public Vector2 GetCursorPosition(int characterIndex, float widthConstraint)
        {
            if (Font == null || string.IsNullOrEmpty(Text) || characterIndex <= 0)
                return Vector2.Zero;

            int safeIndex = Math.Clamp(characterIndex, 0, Text.Length);
            string substring = Text.Substring(0, safeIndex);

            var opts = LayoutOptions;
            opts.MaxWidth = widthConstraint;

            // USE THE NEW METHOD HERE, NOT Measure()!
            return TextMeshGenerator.GetCursorPosition(
                Font,
                substring,
                Font.GetScaleForFontSize(Scale),
                opts
            );
        }

        protected override void InitializeCore()
        {
            Material = UISystem.TextMaterial;
            UpdateMaterial();
        }

        protected override void UpdateMaterial()
        {
            Material?.SetTexture("MAT_fontSDFAtlas", Font!.Atlas.GPUTexture);
            Material?.SetVector4("COLOR", _color);
        }
        //float rainbowTime = 0f;
        protected override void UpdateCore(float deltaTime)
        {
            //rainbowTime += deltaTime * 0.15f; // speed of rainbow

            //float hue = rainbowTime % 1f; // keep hue in [0,1]

            //Color = HSVtoRGB(hue, 1f, 1f);'
        }

        Vector4 HSVtoRGB(float h, float s, float v)
        {
            float r = 0, g = 0, b = 0;

            float i = MathF.Floor(h * 6f);
            float f = h * 6f - i;
            float p = v * (1f - s);
            float q = v * (1f - f * s);
            float t = v * (1f - (1f - f) * s);

            switch ((int)i % 6)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                case 5: r = v; g = p; b = q; break;
            }

            return new Vector4(r, g, b, 1f);
        }

        public override void Dispose()
        {
            _mesh?.Dispose();
            _text = "";

        }
    }
}