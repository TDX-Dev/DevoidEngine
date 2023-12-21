using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace EmberaEngine.Engine.Rendering
{
    public class TextObject
    {
        public Vector2 transform;
        public Vector2 scale;
        public float rotationAngle;
        public Mesh textMesh;
        public Texture fontTexture;
    }

    class TextManager
    {
        public static List<TextObject> TextObjects = new List<TextObject>();

        public static void AddTextComponent(TextObject textObject)
        {
            TextObjects.Add(textObject);
        }

        public static void RemoveTextComponent(TextObject textObject)
        {
            TextObjects.Remove(textObject);
        }

    }
}
