using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.UI
{
    public enum Easing
    {
        EaseOut
    }

    public class UITransition
    {
        public float Duration = 0.25f;
        public Easing Easing = Easing.EaseOut;
    }
}
