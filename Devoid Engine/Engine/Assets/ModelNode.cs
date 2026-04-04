using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Assets
{
    [MessagePackObject]
    public class ModelNode
    {
        [Key(0)] public string Name = "";

        [Key(1)] public int Parent = -1;

        [Key(2)] public int[] MeshIndices = [];

        [Key(3)] public Vector3 Translation;

        [Key(4)] public Quaternion Rotation;

        [Key(5)] public Vector3 Scale;
    }
}
