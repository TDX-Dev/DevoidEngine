using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.UI.Nodes
{
    public class DragIntNode : DragNumberNode<int>
    {
        protected override bool TryParse(string text, out int result)
            => int.TryParse(text, out result);

        protected override string Format(int value)
            => value.ToString();

        protected override int Add(int value, float delta)
            => value + (int)delta;

        protected override void ApplyDelta(float delta)
        {
            int step = (int)MathF.Round(delta);

            if (step != 0)
                Value += step;
        }
    }
}
