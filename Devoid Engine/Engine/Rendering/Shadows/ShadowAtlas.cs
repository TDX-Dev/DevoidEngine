using DevoidEngine.Engine.Core;
using DevoidGPU;

namespace DevoidEngine.Engine.Rendering.Shadows
{
    public class ShadowAtlas
    {
        public Texture2D DepthTexture;
        public Framebuffer Framebuffer;

        int atlasSize;
        int tileSize;
        int tilesPerRow;

        int currentTile = 0;

        public ShadowAtlas(int atlasSize = 2048, int tileSize = 1024)
        {
            this.atlasSize = atlasSize;
            this.tileSize = tileSize;

            tilesPerRow = atlasSize / tileSize;

            DepthTexture = new Texture2D(new TextureDescription()
            {
                Width = atlasSize,
                Height = atlasSize,
                Format = TextureFormat.Depth32_Float,
                IsDepthStencil = true,
                IsRenderTarget = false,
                MipLevels = 1
            });

            Framebuffer = new Framebuffer();
            Framebuffer.AttachDepthTexture(DepthTexture);
        }

        public void Reset()
        {
            currentTile = 0;
        }

        public int AllocateTile(out float offsetX, out float offsetY, out float scale)
        {
            int tile = currentTile++;

            int x = tile % tilesPerRow;
            int y = tile / tilesPerRow;

            scale = (float)tileSize / atlasSize;

            offsetX = x * scale;
            offsetY = y * scale;

            return tile;
        }

        public void SetViewport(int tile)
        {
            int x = tile % tilesPerRow;
            int y = tile / tilesPerRow;

            Renderer.GraphicsDevice.SetViewport(
                x * tileSize,
                y * tileSize,
                tileSize,
                tileSize);
        }
    }
}