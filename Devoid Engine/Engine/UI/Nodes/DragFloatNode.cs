using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.UI.Nodes
{
    public class DragFloatNode : DragNumberNode<float>
    {
        protected override bool TryParse(string text, out float result)
            => float.TryParse(text, out result);

        protected override string Format(float value)
            => value.ToString("0.###");

        protected override float Add(float value, float delta)
            => value + delta;

        protected override void ApplyDelta(float delta)
        {
            Value += delta;
        }
    }
}
