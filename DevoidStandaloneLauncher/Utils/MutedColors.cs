using System.Numerics;

namespace DevoidStandaloneLauncher.Utils
{
    public static class MutedColors
    {
        private static Random _rng = new Random();

        public static readonly List<Vector4> Colors = new List<Vector4>()
    {
        new Vector4(0.55f, 0.48f, 0.45f, 1f), // dusty brown
        new Vector4(0.46f, 0.53f, 0.50f, 1f), // muted teal
        new Vector4(0.60f, 0.52f, 0.45f, 1f), // warm taupe
        new Vector4(0.52f, 0.50f, 0.60f, 1f), // soft purple gray
        new Vector4(0.48f, 0.58f, 0.47f, 1f), // moss green
        new Vector4(0.60f, 0.47f, 0.47f, 1f), // dusty red
        new Vector4(0.50f, 0.55f, 0.60f, 1f), // muted blue gray
        new Vector4(0.62f, 0.55f, 0.48f, 1f), // sand
        new Vector4(0.45f, 0.50f, 0.55f, 1f), // slate
        new Vector4(0.58f, 0.50f, 0.52f, 1f), // rose gray
        new Vector4(0.52f, 0.57f, 0.52f, 1f), // sage
        new Vector4(0.50f, 0.48f, 0.42f, 1f)  // olive gray
    };

        public static Vector4 GetRandom()
        {
            return Colors[_rng.Next(Colors.Count)];
        }
    }
}
