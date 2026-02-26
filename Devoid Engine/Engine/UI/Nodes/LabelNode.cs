using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.UI.Text;
using System.Numerics;

namespace DevoidEngine.Engine.UI.Nodes
{
    public class LabelNode : UINode
    {
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;

                var newMesh = TextMeshGenerator.Generate(Font, _text, Font.GetScaleForFontSize(Scale));
                Size = TextMeshGenerator.Measure(Font, _text, Font.GetScaleForFontSize(Scale));

                var oldMesh = _mesh;
                _mesh = newMesh;

                if (oldMesh != null)
                    oldMesh.Dispose(); // this internally defers GPU delete
            }
        }
        public FontInternal Font;
        public float Scale = 1f;

        private Vector2 _measuredTextSize;
        private Mesh _mesh;
        private string _text;

        public LabelNode(string text, FontInternal font, float scale = 1f)
        {
            Font = font;
            Scale = scale;
            Text = text;

            // Text should NOT expand by default
            Layout.FlexGrowMain = 0;
            Layout.FlexGrowCross = 0;
        }

        protected override Vector2 MeasureCore(Vector2 availableSize)
        {
            if (Font == null || string.IsNullOrEmpty(Text))
                return Vector2.Zero;

            return Size ?? Vector2.Zero;
        }

        protected override void ArrangeCore(UITransform finalRect)
        {
            Rect = finalRect;

            if (Font == null || string.IsNullOrEmpty(Text) || _mesh == null)
                return;


            // Render text inside the allocated rect
            //UIRenderer.DrawText(
            //    new UITransform(
            //        finalRect.position,
            //        finalRect.size
            //    ),
            //    _mesh,
            //    Font.Atlas.GPUTexture
            //);
        }

        protected override void RenderCore(List<RenderItem> renderList, Matrix4x4 canvasModel)
        {
            Matrix4x4 local = UISystem.BuildTranslationModel(Rect);
            Matrix4x4 final = local * canvasModel;

            renderList.Add(new RenderItem()
            {
                Mesh = _mesh,
                Material = Material,
                Model = final
            });

            // Not used — rendering happens in ArrangeCore like BoxNode
        }

        protected override void InitializeCore()
        {
            Material = UISystem.TextMaterial;
            Material.SetTexture("MAT_fontSDFAtlas", Font.Atlas.GPUTexture);
            
        }
    }
}
