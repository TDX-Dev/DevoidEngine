using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.UI.Nodes;
using DevoidEngine.Engine.Utilities;
using System.Numerics;

namespace DevoidEngine.Engine.UI
{
    public static class UISystem
    {
        public static List<UINode> Roots = new();

        public static UINode FocusedNode { get; private set; }

        private static Material uiMaterial;
        private static Material textMaterial;

        public static Mesh QuadMesh;

        public static MaterialInstance UIMaterial => new MaterialInstance(uiMaterial);
        public static MaterialInstance TextMaterial => new MaterialInstance(textMaterial);

        public static void Initialize()
        {
            QuadMesh = new Mesh();
            QuadMesh.SetVertices(Primitives.GetQuadVertex());

            uiMaterial = new Material(
                new Shader("Engine/Content/Shaders/UI/basic")
            );

            textMaterial = new Material(
                new Shader(
                    "Engine/Content/Shaders/UI/basic.vert.hlsl",
                    "Engine/Content/Shaders/UI/sdf_text.frag.hlsl"
                )
            );

            foreach (var root in Roots)
                root.Initialize();
        }

        public static void AddRoot(UINode node)
        {
            Roots.Add(node);
            node.Initialize();
        }

        public static void RemoveRoot(UINode node)
        {
            Roots.Remove(node);
        }

        public static void SetFocus(UINode node)
        {
            if (FocusedNode == node)
                return;

            //FocusedNode?.OnBlur();

            FocusedNode = node;

            //FocusedNode?.OnFocus();
        }

        public static void Update(float deltaTime)
        {


            Vector2 screen = Screen.Size;

            foreach (var root in Roots)
            {
                root.Update(deltaTime);
                root.Measure(screen);
                root.Arrange(new UITransform(Vector2.Zero, screen));
            }
        }

        private static UINode HitTest(UINode node, Vector2 position)
        {
            if (!node.Visible)
                return null;

            for (int i = node._children.Count - 1; i >= 0; i--)
            {
                var hit = HitTest(node._children[i], position);
                if (hit != null)
                    return hit;
            }

            if (!node.BlockInput)
            {
                return null;
            }

            var rect = node.Rect;

            //Console.WriteLine(node.GetType().Name);
            //Console.WriteLine(rect.position);
            //Console.WriteLine(rect.size);
            //Console.WriteLine(position);

            if (position.X >= rect.position.X &&
                position.X <= rect.position.X + rect.size.X &&
                position.Y >= rect.position.Y &&
                position.Y <= rect.position.Y + rect.size.Y)
            {
                return node;
            }

            return null;
        }

        public static Matrix4x4 BuildModel(UITransform t)
        {
            return
                Matrix4x4.CreateScale(t.size.X, t.size.Y, 1f) *
                Matrix4x4.CreateTranslation(t.position.X, t.position.Y, 0f);
        }

        public static Matrix4x4 BuildTranslationModel(UITransform t)
        {
            return Matrix4x4.CreateTranslation(
                t.position.X,
                t.position.Y,
                0f
            );
        }
    }
}