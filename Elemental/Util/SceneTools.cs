using DevoidEngine.Attributes;
using DevoidEngine.Components;
using DevoidEngine.Core;
using DevoidEngine.Serialization;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elemental.Util
{
    public static class SceneTools
    {
        public static Scene Copy(Scene scene)
        {
            SceneData data = SceneSerializer.Serialize(scene);
            byte[] scenebytes = MessagePackSerializer.Serialize(data);
            SceneData copiedData = MessagePackSerializer.Deserialize<SceneData>(scenebytes);
            Scene copiedScene = SceneSerializer.Deserialize(copiedData);
            copiedScene.SceneName = "Copied Scene";
            return copiedScene;

        }

        public static byte[] Serialize(Scene scene)
        {
            SceneData data = SceneSerializer.Serialize(scene);
            byte[] scenebytes = MessagePackSerializer.Serialize(data);
            return scenebytes;
        }
    }
}
