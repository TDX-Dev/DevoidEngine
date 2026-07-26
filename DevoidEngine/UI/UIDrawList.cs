using DevoidEngine.Core;
using DevoidEngine.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.UI
{
    public class UIDrawList
    {
        public List<UICommand> Commands = [];

        public void PushTransform(Matrix4x4 matrix)
        {
            Commands.Add(new UICommand()
            {
                Type = UICommandType.PushTransform,
                Transform = new TransformCommand()
                {
                    Transform = matrix
                }
            });
        }

        public void PopTransform()
        {
            Commands.Add(new UICommand()
            {
                Type = UICommandType.PopTransform
            });
        }

        public void AddQuad(Rect rect, MaterialInstance material, int Order, float Rotation, Vector2 PivotOffset)
        {
            Commands.Add(new UICommand()
            {
                Type = UICommandType.Quad,
                Quad = new QuadCommand()
                {
                    Order = Order,
                    Rect = rect,
                    Material = material,
                    Rotation = Rotation,
                    PivotOffset = PivotOffset
                }
            });
        }

        public void AddText(Rect rect, Mesh textMesh, MaterialInstance material, int Order, float Rotation, Vector2 PivotOffset)
        {
            Commands.Add(new UICommand()
            {
                Type = UICommandType.Text,
                Text = new TextCommand()
                {
                    Order = Order,
                    Rect = rect,
                    TextMesh = textMesh,
                    Material = material,
                    Rotation = Rotation,
                    PivotOffset = PivotOffset
                }
            });
        }

        public void Clear()
        {
            Commands.Clear();
        }
    }
}
