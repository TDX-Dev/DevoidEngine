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
    public struct UICommand
    {
        public UICommandType Type;

        public TransformCommand Transform;
        public QuadCommand Quad;
        public TextCommand Text;
        public ClipCommand Clip;
    }

    public struct TransformCommand
    {
        public Matrix4x4 Transform;
    }

    public struct TextCommand
    {
        public int Order;
        public Rect Rect;
        public Mesh TextMesh;
        public MaterialInstance Material;
        public float Rotation;
        public Vector2 PivotOffset;
    }

    public struct QuadCommand
    {
        public int Order;
        public Rect Rect;
        public float Rotation;
        public MaterialInstance Material;
        public Vector2 PivotOffset;
    }

    public struct ClipCommand
    {
        public Rect Rect;
    }
}
