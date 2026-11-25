using DevoidEngine.Core;
using DevoidEngine.Logging;
using DevoidEngine.Util;

namespace DevoidEngine.Nodes
{
    public class MeshInstance3D : Node
    {
        public Mesh? Mesh;
        private RID meshInstance;

        protected override void OnTreeEntered()
        {
            CreateMeshResources();
        }

        protected override void OnTreeExited()
        {

        }

        protected override void OnStart()
        {
            CreateMeshResources();
            DevoidLog.Debug("MeshInstanceNode started");
        }

        void CreateMeshResources()
        {
            if (Mesh != null && !meshInstance.IsValid)
                meshInstance = Scene.RenderWorld.CreateMeshInstance(Mesh);
        }
    }
}
