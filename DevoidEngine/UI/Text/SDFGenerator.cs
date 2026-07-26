using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.UI.Text
{
    public static class SDFGenerator
    {
        public static GlyphSDFImage Generate(byte[] bitmap, int width, int height, int pitch, int padding, int pixelRange, int scale)
        {
            padding *= scale;
            pixelRange *= scale;

            int paddedWidth = (int)(width + padding * 2);
            int paddedHeight = (int)(height + padding * 2);

            float[] grid = new float[paddedWidth * paddedHeight];

            const float INF = 1e7f;

            Array.Fill(grid, INF);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte value = bitmap[y * pitch + x];
                    if (value > 64)
                    {
                        grid[(y + padding) * paddedWidth + (x + padding)] = 0f;
                    }
                }
            }

            float[] distOutside = PerformEDT(grid, paddedWidth, paddedHeight);

            for (int i = 0; i < grid.Length; i++) grid[i] = (distOutside[i] == 0) ? INF : 0f;

            float[] distInside = PerformEDT(grid, paddedWidth, paddedHeight);

            byte[] output = new byte[paddedWidth * paddedHeight];

            float invRange = 1.0f / pixelRange;

            for (int i = 0; i < output.Length; i++)
            {
                float sd =
                    MathF.Sqrt(distInside[i]) -
                    MathF.Sqrt(distOutside[i]);

                // Clamp to supported range
                sd = Math.Clamp(sd, -pixelRange, pixelRange);

                // [-range, range] -> [0, 1]
                float normalized = sd * invRange * 0.5f + 0.5f;

                output[i] = (byte)(normalized * 255.0f + 0.5f);
            }

            byte[] finalSdf = Downsample(
                output,
                paddedWidth,
                paddedHeight,
                scale
            );

            int finalWidth = paddedWidth / scale;
            int finalHeight = paddedHeight / scale;

            return new GlyphSDFImage
            {
                SDF = finalSdf,
                Width = finalWidth,
                Height = finalHeight,
                Padding = padding / scale
            };

        }

        private static byte[] Downsample(byte[] highResSdf, int highWidth, int highHeight, int scaleFactor)
        {
            int lowWidth = highWidth / scaleFactor;
            int lowHeight = highHeight / scaleFactor;
            byte[] lowResSdf = new byte[lowWidth * lowHeight];

            for (int y = 0; y < lowHeight; y++)
            {
                for (int x = 0; x < lowWidth; x++)
                {
                    // Simple Box Filter (Average pooling)
                    int sum = 0;
                    for (int sy = 0; sy < scaleFactor; sy++)
                    {
                        for (int sx = 0; sx < scaleFactor; sx++)
                        {
                            int highX = (x * scaleFactor) + sx;
                            int highY = (y * scaleFactor) + sy;
                            sum += highResSdf[highY * highWidth + highX];
                        }
                    }

                    lowResSdf[y * lowWidth + x] = (byte)(sum / (scaleFactor * scaleFactor));
                }
            }

            return lowResSdf;
        }

        private static float[] PerformEDT(float[] grid, int w, int h)
        {
            float[] dist = new float[w * h];
            Array.Copy(grid, dist, grid.Length);

            int maxDim = Math.Max(w, h);

            float[] dt = new float[maxDim];
            float[] d_out = new float[maxDim];
            int[] v = new int[maxDim];
            float[] z = new float[maxDim + 1];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++) dt[y] = dist[y * w + x];

                Solve1D(dt, d_out, h, v, z);

                for (int y = 0; y < h; y++) dist[y * w + x] = d_out[y];
            }

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++) dt[x] = dist[y * w + x];

                Solve1D(dt, d_out, w, v, z);

                for (int x = 0; x < w; x++) dist[y * w + x] = d_out[x];
            }

            return dist;
        }

        private static void Solve1D(float[] f, float[] d, int n, int[] v, float[] z)
        {
            int k = 0;
            v[0] = 0;
            z[0] = float.NegativeInfinity;
            z[1] = float.PositiveInfinity;

            for (int q = 1; q < n; q++)
            {
                while (true)
                {
                    int vk = v[k];
                    float s = ((f[q] + q * q) - (f[vk] + vk * vk)) / (2 * (q - vk));

                    if (s <= z[k])
                    {
                        k--;
                        continue;
                    }

                    k++;
                    v[k] = q;
                    z[k] = s;
                    z[k + 1] = float.PositiveInfinity;
                    break;
                }
            }

            k = 0;
            for (int q = 0; q < n; q++)
            {
                while (z[k + 1] < q) k++;
                int vk = v[k];
                float dx = q - vk;
                d[q] = dx * dx + f[vk];
            }
        }
    }
}
