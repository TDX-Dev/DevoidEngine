using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;

namespace DevoidEngine.Engine.Serialization
{
    public delegate void ComponentFieldSetter(Component owner, Component value);
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

        class PendingComponentReference
        {
            public Component Owner = null!;
            public ComponentFieldSetter Setter = null!;
            public Guid GameObjectId;
            public string ComponentType = "";
        }

        class PendingGameObjectReference
        {
            public Component Owner = null!;
            public Action<Component, GameObject?> Setter = null!;
            public Guid TargetId;
        }

        static readonly List<PendingComponentReference> pending = new();
        static readonly List<PendingGameObjectReference> pendingGameObjects = new();

        public static void RegisterComponentReference(
            Component owner,
            ComponentFieldSetter setter,
            Guid goId,
            string type)
        {
            pending.Add(new PendingComponentReference
            {
                Owner = owner,
                Setter = setter,
                GameObjectId = goId,
                ComponentType = type
            });
        }

        public static void ResolveComponentReferences(Scene scene)
        {
            foreach (var p in pending)
            {
                var go = scene.GetGameObject(p.GameObjectId);
                if (go == null)
                    continue;

                var component =
                    ComponentSerializationRegistry.FindComponent(go, p.ComponentType);

                if (component == null)
                    continue;

                p.Setter(p.Owner, component);
            }

            pending.Clear();
        }

        public static void RegisterGameObjectReference(
            Component owner,
            Action<Component, GameObject?> setter,
            Guid targetId
        )
        {
            pendingGameObjects.Add(new PendingGameObjectReference
            {
                Owner = owner,
                Setter = setter,
                TargetId = targetId
            });
        }

        public static void ResolveGameObjectReferences(Scene scene)
        {
            foreach (var p in pendingGameObjects)
            {
                var go = scene.GetGameObject(p.TargetId);
                p.Setter(p.Owner, go);
            }

            pendingGameObjects.Clear();
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
