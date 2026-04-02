using DevoidEngine.Engine.UI.Nodes;
using SharpFont;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.UI
{
    public class DragVector3Node : ContainerNode
    {
        public DragFloatNode X = new();
        public DragFloatNode Y = new();
        public DragFloatNode Z = new();

        public Vector3 Value
        {
            get => new(X.Value, Y.Value, Z.Value);
            set
            {
                X.Value = value.X;
                Y.Value = value.Y;
                Z.Value = value.Z;
            }
        }

        public DragVector3Node()
        {
            Direction = FlexDirection.Row;
            Gap = 4;

            Add(X);
            Add(Y);
            Add(Z);
        }
    }
}
