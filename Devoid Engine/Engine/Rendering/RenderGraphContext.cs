using DevoidEngine.Engine.Core;

namespace DevoidEngine.Engine.Rendering
{
    public class RenderGraphContext
    {
        Dictionary<string, Texture2D> textures = new();

        public void SetTexture(string name, Texture2D texture)
        {
            textures[name] = texture;
        }

        public Texture2D GetTexture(string name)
        {
            if (textures.TryGetValue(name, out var tex))
            {
                return tex;
            }
            else
            {
                return Texture2D.BlackTexture;
            }
        }
    }
}