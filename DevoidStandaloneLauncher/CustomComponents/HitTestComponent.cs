using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Physics;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Utilities;
using System.Diagnostics;
using System.Numerics;

namespace DevoidEngine.Engine.Components
{
    public class HitMarkerComponent : Component
    {
        public override string Type => nameof(HitMarkerComponent);

        public float RayDistance = 100f;
        public Vector3 MarkerScale = new Vector3(0.3f);

        private GameObject markerObject;
        private Mesh markerMesh;

        public override void OnStart()
        {


            Console.WriteLine("Called!");

            // Create a simple cube mesh
            markerMesh = new Mesh();
            markerMesh.SetVertices(Primitives.GetCubeVertex());

            // Create marker object
            markerObject = gameObject.Scene.addGameObject("HitMarker");
            markerObject.transform.Scale = MarkerScale;

            markerObject.AddComponent<MeshRenderer>().AddMesh(markerMesh);
        }

        public override void OnUpdate(float dt)
        {
            Transform camTransform = gameObject.transform;

            Vector3 origin = camTransform.Position;
            Vector3 direction = camTransform.Forward;

            markerObject.transform.Position = origin + direction * 2;

            if (gameObject.Scene.Physics.Raycast(
                new Ray(origin + direction, direction),
                RayDistance,
                out RaycastHit hit))
            {
                // Move marker to hit point
                markerObject.transform.Position = hit.Point;

                // Optional: Align to surface normal
                AlignToNormal(hit.Normal);
            }
            //else
            //{
            //    // Hide marker if nothing hit
            //    markerObject.transform.Position = origin + direction * RayDistance;
            //}
        }

        private void AlignToNormal(Vector3 normal)
        {
            normal = Vector3.Normalize(normal);

            // Build rotation from normal
            Quaternion rotation = Quaternion.CreateFromRotationMatrix(
                Matrix4x4.CreateWorld(
                    Vector3.Zero,
                    normal,
                    Vector3.UnitY
                )
            );

            markerObject.transform.Rotation = rotation;
        }
    }
}