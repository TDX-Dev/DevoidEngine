using DevoidEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Serialization
{
    public static class SceneSerializer
    {
        public static SceneData Serialize(Scene scene)
        {
            SceneData data = new SceneData();

            foreach (var go in scene.GameObjects)
            {
                data.GameObjects.Add(
                    GameObjectSerializer.Serialize(go));
            }

            return data;
        }

        public static Scene Deserialize(SceneData data)
        {
            Scene scene = new Scene();

            Dictionary<Guid, GameObject> map = new();

            // pass 1: create objects
            foreach (var goData in data.GameObjects)
            {
                var go = GameObjectSerializer.Deserialize(goData, scene, map);
                scene.AddGameObject(go);
            }

            // pass 2: rebuild hierarchy
            foreach (var goData in data.GameObjects)
            {
                if (goData.Parent == Guid.Empty)
                    continue;

                var go = map[goData.Id];
                var parent = map[goData.Parent];

                go.parentObject = parent;
                parent.children.Add(go);
            }

            return scene;
        }
    }
}
