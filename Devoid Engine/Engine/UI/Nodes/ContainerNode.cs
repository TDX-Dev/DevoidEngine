using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using System.Numerics;

namespace DevoidEngine.Engine.UI.Nodes
{
    public class ContainerNode : FlexboxNode
    {
        private Texture2D _texture;

        public Texture2D Texture
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

        public Vector4 BorderRadius
        {
            get => _borderRadius;
            set
            {
                _borderRadius = value;
                UpdateMaterial();
            }
        }


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

        }

        //protected override Vector2 MeasureCore(Vector2 availableSize)
        //{
        //    return Size ?? Vector2.Zero;
        //}

        //protected override void ArrangeCore(UITransform finalRect)
        //{
        //    Rect = finalRect;
        //}

        //protected override void RenderCore(List<RenderItem> renderList, Matrix4x4 canvasModel, int order)
        //{
        //    Matrix4x4 local = Matrix4x4.CreateRotationZ(Rotation) * UISystem.BuildModel(Rect) * Matrix4x4.CreateTranslation(0, 0, order * 0.001f);
        //    Matrix4x4 final = local * canvasModel;

        //    renderList.Add(new RenderItem()
        //    {
        //        Mesh = UISystem.QuadMesh,
        //        Material = Material,
        //        Model = final
        //    });
        //}

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
                Matrix4x4.CreateTranslation(centerPos.X, centerPos.Y, order * 0.001f);

            Matrix4x4 final = model * canvasModel;

            renderList.Add(new RenderItem()
            {
                Mesh = UISystem.QuadMesh,
                Material = Material,
                Model = final
            });
            //DebugRenderSystem.DrawRectUI(model);
        }

        protected override void UpdateCore(float deltaTime)
        {

        }
    }
}