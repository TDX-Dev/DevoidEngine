using MessagePack;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace DevoidEngine.Engine.Assets
{
    [MessagePackObject]
    public class MaterialAsset
    {
        [Key(0)]
        public string Shader = "";

        [Key(1)]
        public Dictionary<string, Guid> Textures = new();

        [Key(2)]
        public Dictionary<string, int> Ints = new();

        [Key(3)]
        public Dictionary<string, float> Floats = new();

        [Key(4)]
        public Dictionary<string, Vector2> Vector2s = new();

        [Key(5)]
        public Dictionary<string, Vector3> Vector3s = new();

        [Key(6)]
        public Dictionary<string, Vector4> Vector4s = new();

        [Key(7)]
        public Dictionary<string, Matrix4x4> Matrices = new();
    }
}