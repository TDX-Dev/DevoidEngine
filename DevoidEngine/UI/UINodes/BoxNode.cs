using DevoidEngine.Core;
using System.Numerics;

namespace DevoidEngine.UI.UINodes
{
    public class BoxNode : FlexboxNode
    {
        private Texture? _texture;

        public Texture? Texture
        {
            get => _texture;
            set
            {
                _texture = value;
                UpdateMaterial();
            }
        }

        public Vector4 Color
        {
            get => _color;
            set
            {
                _color = value;
                UpdateMaterial();
            }
        }

        public float Opacity
        {
            get => _opacity;
            set
            {
                _opacity = value;
                UpdateMaterial();
            }
        }

        public float BorderThickness = 0f;
        public Vector4 BorderColor = new(0, 0, 0, 1);

        private Vector4 _color = new(1, 1, 1, 1);
        private float _opacity = 1f;

        protected override void InitializeCore()
        {
            Material = Engine.UISystem.DefaultBoxMaterial;
            UpdateMaterial();
        }

        protected override void UpdateMaterial()
        {
            if (Material == null)
                return;

            Material.SetInt("useTexture", _texture != null ? 1 : 0);
            if (_texture != null)
                Material.SetTexture("MAT_Texture", _texture);

            Vector4 final = _color;
            final.W *= _opacity;

            Material.SetVector4("COLOR", final);
            Material.SetVector2("RECT_SIZE", Rect.Size);
        }

        //protected override Vector2 MeasureCore(Vector2 availableSize)
        //{
        //    return Size ?? Vector2.Zero;
        //}

        //protected override void ArrangeCore(UITransform finalRect)
        //{
        //    Rect = finalRect;
        //}

        protected override void PreRender(UIDrawList drawList, int order)
        {
            if (Material == null)
                return;
            //Matrix4x4 local = UISystem.BuildTranslationModel(Rect!) * Matrix4x4.CreateTranslation(0, 0, order * 0.001f);
            //Matrix4x4 final = local * canvasModel;

            drawList.AddQuad(Rect, Material, order, Rotation, Vector2.Zero);

            //renderList.Add(new RenderItem()
            //{
            //    Mesh = UISystem.QuadMesh,
            //    Material = Material,
            //    Model = UISystem.BuildModel(Rect!)
            //});
        }

        protected override void UpdateCore(float deltaTime)
        {

        }
    }
}
