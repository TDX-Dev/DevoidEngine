using DevoidEngine.Assets;
using DevoidEngine.Attributes;
using DevoidEngine.Util;
using DevoidGPU;

namespace DevoidEngine.Core
{
    [DevoidClass]
    public sealed class Mesh : AssetType
    {
        public ResourceUsage Usage { get; }

        public float UniqueIdentifier = float.MaxValue;

        public MeshSurface[] Surfaces { get; set; } = [];

        public BoundingBox LocalBounds { get; private set; }

        public Mesh(ResourceUsage usage = ResourceUsage.Default)
        {
            Usage = usage;
        }

        public void Upload(bool computeLocalBounds = true)
        {
            if (Surfaces.Length == 0)
                throw new InvalidOperationException("Mesh must have at least one surface");

            for (int i = 0; i < Surfaces.Length; i++)
            {
                MeshSurface? surface = Surfaces[i] ?? throw new InvalidOperationException($"Mesh contains null surface at index {i}");
                surface.Upload(computeLocalBounds);
            }

            if (computeLocalBounds)
                ComputeLocalBounds();
        }

        private void ComputeLocalBounds()
        {
            if (Surfaces.Length == 0)
            {
                LocalBounds = BoundingBox.Empty;
                return;
            }

            BoundingBox bounds = Surfaces[0].LocalBounds;

            for (int i = 1; i < Surfaces.Length; i++)
                bounds = BoundingBox.Union(bounds, Surfaces[i].LocalBounds);

            LocalBounds = bounds;
        }

        public void Draw(ICommandList cmd)
        {
            foreach (MeshSurface surface in Surfaces)
                surface.Draw(cmd);
        }

        public void DrawInstanced(ICommandList cmd, int instanceCount)
        {
            foreach (MeshSurface surface in Surfaces)
                surface.DrawInstanced(cmd, instanceCount);
        }

        public override void Dispose()
        {
            foreach (MeshSurface surface in Surfaces)
                surface.Dispose();

            Surfaces = [];
        }
    }
}