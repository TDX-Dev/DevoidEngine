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

            try
            {
                foreach (var goData in data.GameObjects)
                {
                    try
                    {
                        var go = GameObjectSerializer.Deserialize(goData, scene, map);
                        scene.AddGameObject(go);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"[Scene] Failed to load GameObject {goData.Name}: {e.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Scene] Scene corrupted: {e.Message}");
            }

            foreach (var goData in data.GameObjects)
            {
                if (goData.Parent == Guid.Empty)
                    continue;

                if (!map.TryGetValue(goData.Id, out var child))
                    continue;

                if (!map.TryGetValue(goData.Parent, out var parent))
                    continue;

                //child.Transform.SetParent(parent.Transform);
                child.SetParent(parent, false);
            }

            GameObjectSerializer.ResolveComponentReferences(scene);

            return scene;
        }
    }
}
