using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.UI.Nodes;
using DevoidEngine.Engine.Utilities;
using System.Numerics;

namespace DevoidEngine.Engine.UI
{
    public static class UISystem
    {
        public static List<UINode> Roots;

        public static MaterialInstance UIMaterial
        {
            get => new MaterialInstance(uiMaterial);
        }
        public static MaterialInstance TextMaterial
        {
            get => new MaterialInstance(textMaterial);
        }

        private static Material uiMaterial;
        private static Material textMaterial;

        public static Mesh QuadMesh;

        static UISystem()
        {
            Roots = new List<UINode>();
        }

        public static void Initialize()
        {
            QuadMesh = new Mesh();
            QuadMesh.SetVertices(Primitives.GetQuadVertex());

            uiMaterial = new Material(new Shader("Engine/Content/Shaders/UI/basic"));

            textMaterial = new Material(new Shader("Engine/Content/Shaders/UI/basic.vert.hlsl", "Engine/Content/Shaders/UI/sdf_text.frag.hlsl"));

            for (int i = 0; i < Roots.Count; i++)
            {
                Roots[i].Initialize();
            }
        }

        public static Matrix4x4 BuildModel(UITransform t)
        {
            return
                Matrix4x4.CreateScale(t.size.X, t.size.Y, 1f) *
                Matrix4x4.CreateTranslation(t.position.X, t.position.Y, 0f);
        }

        public static Matrix4x4 BuildTranslationModel(UITransform t)
        {
            return Matrix4x4.CreateTranslation(t.position.X, t.position.Y, 0f);
        }


        public static void Update()
        {

            for (int i = 0; i < Roots.Count; i++)
            {
                Roots[i].Measure(Screen.Size);
                Roots[i].Arrange(new UITransform(Vector2.Zero, Screen.Size));
            }

        }
    }
}
