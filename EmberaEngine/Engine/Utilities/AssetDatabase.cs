using System;
using System.Collections.Generic;
using EmberaEngine.Engine.Core;

namespace EmberaEngine.Engine.Utilities
{
    class AssetDatabase
    {
        struct ShaderAsset
        {
            public string filename;
            public Shader Shader;
            public int UUID;
        }

        struct MeshAsset
        {
            public string filename;
            public Mesh Mesh;
            public int UUID;
        }

        static List<MeshAsset> MeshAssets = new List<MeshAsset>();
        static List<ShaderAsset> ShaderAssets = new List<ShaderAsset>();

        public static int Add(Mesh mesh, string filename, int uuid)
        {
            int UUID = CreateRandomNum();
            MeshAssets.Add(new MeshAsset()
            {
                Mesh = mesh,
                filename = filename,
                UUID = UUID
            });
            return UUID;
        }

        public static int Add(Shader shader, string filename, int uuid)
        {
            Console.WriteLine(System.IO.Path.GetFileNameWithoutExtension(filename));

            int UUID = CreateRandomNum();
            ShaderAssets.Add(new ShaderAsset()
            {
                Shader = shader,
                filename = System.IO.Path.GetFileName(filename),
                UUID = UUID
            });
            return UUID;
        }

        public static Shader Get(string filename)
        {
            foreach (ShaderAsset asset in ShaderAssets)
            {
                if (asset.filename == filename)
                {
                    Console.WriteLine("Found");
                    return asset.Shader;
                }
            }
            return null;
        }

        static int CreateRandomNum()
        {
            int rnd = UtilRandom.Next();
            return rnd;
        }
    }
}
