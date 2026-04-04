using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Assets
{
    [MessagePackObject]
    public class MeshAsset
    {
        [Key(0)] public float[] Positions = [];
        [Key(1)] public float[] Normals = [];
        [Key(2)] public float[] UVs = [];
        [Key(3)] public float[] Tangents = [];
        [Key(4)] public float[] Bitangents = [];
        [Key(5)] public uint[] Indices = [];

        [Key(6)] public int MaterialIndex = -1;
    }
}
