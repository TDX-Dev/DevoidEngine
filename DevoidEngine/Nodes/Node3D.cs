using DevoidEngine.Attributes;
using DevoidEngine.Core;
using System.Numerics;

namespace DevoidEngine.Nodes
{
    public class Node3D : Node
    {
        [HideInInspector]
        public Transform3D Transform { get; set; }

        public Node3D()
        {
            Transform = new Transform3D();
            
        }

        public override void SetParent(Node? node, bool keepWorldTransform = false)
        {
            base.SetParent(node, keepWorldTransform);

            // This is currently a weak implementation, since if Node.SetParent is called upon while boxed
            // Then the cast will fail.
            // TODO: Make base class Transform from which Transform2D and Transform3D inherit, and handle in
            // base Node class
            Transform.SetParent(((Node3D?)node)?.Transform, keepWorldTransform); 
        }

        public override void InitializeTransforms()
        {
            Transform.InitializeInterpolation();
        }

        public override void ClearTransform()
        {
            Transform.ClearDirty();
        }

        public override void CapturePreviousTransform()
        {
            Transform.CapturePrevious();
        }
    }
}
