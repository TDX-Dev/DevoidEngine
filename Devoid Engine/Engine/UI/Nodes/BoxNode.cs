using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.UI;
using DevoidEngine.Engine.UI.Nodes;
using System.Numerics;

namespace DevoidEngine.Engine.UI.Nodes
{
    public class BoxNode : UINode
    {
        public Texture2D Texture
        {
            get => _texture;
            set
            {
                _texture = value;
            }
        }

        Texture2D _texture;

        protected override Vector2 MeasureCore(Vector2 availableSize)
        {
            return Size ?? Vector2.Zero;
        }

        protected override void ArrangeCore(UITransform finalRect)
        {
            Rect = finalRect;
            //UIRenderer.DrawRect(finalRect, DEBUG_NUM_LOCAL);
        }

        protected override void RenderCore(List<RenderItem> renderList, Matrix4x4 canvasModel)
        {
            renderList.Add(new RenderItem()
            {
                Mesh = UISystem.QuadMesh,
                Material = Material,
                Model = UISystem.BuildModel(Rect),
            });
        }

        protected override void InitializeCore()
        {
            Material = UISystem.UIMaterial;

            Vector3 color = new Vector3(
                (DEBUG_NUM_LOCAL * 16807u % 255) / 255.0f,
                (DEBUG_NUM_LOCAL * 48271u % 255) / 255.0f,
                (DEBUG_NUM_LOCAL * 69621u % 255) / 255.0f
            );

            Material.SetInt("useTexture", _texture != null ? 1 : 0);
            Material.SetTexture("MAT_Texture", _texture);

            Material.SetVector4("COLOR", new Vector4(color, 1));
        }
    }

}