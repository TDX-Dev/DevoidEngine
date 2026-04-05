using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;

namespace DevoidEngine.Engine.Serialization
{
    public static class GameObjectSerializer
    {
        public static GameObjectData Serialize(GameObject go)
        {
            GameObjectData data = new GameObjectData();

            data.Id = go.Id;
            data.Name = go.Name;

            data.Parent = go.parentObject?.Id ?? Guid.Empty;

            data.Position = go.Transform.LocalPosition;
            data.Rotation = go.Transform.LocalRotation;
            data.Scale = go.Transform.LocalScale;

            foreach (var component in go.Components)
            {
                if (component is Transform3D)
                    continue;

                data.Components.Add(new ComponentData
                {
                    Type = component.GetType().FullName!,
                    Data = ComponentSerializationRegistry.Serialize(component)
                });
            }

            return data;
        }

        public static GameObject Deserialize(
            GameObjectData data,
            Scene scene,
            Dictionary<Guid, GameObject> map
        )
        {
            GameObject go = new GameObject();

            go.Id = data.Id;
            go.Name = data.Name;
            go.Scene = scene;
            var transform = go.AddComponent<Transform3D>();
            go.Transform = transform;

            transform.LocalPosition = data.Position;
            transform.LocalRotation = data.Rotation;
            transform.LocalScale = data.Scale;


            map[data.Id] = go;

            foreach (var compData in data.Components)
            {
                var component = ComponentSerializationRegistry.Deserialize(
                    compData.Type,
                    compData.Data);

                if (component == null)
                {
                    Console.WriteLine($"[Scene] Skipping component {compData.Type}");
                    continue;
                }

                go.AddComponent(component);
            }


            return go;
        }
    }
}
