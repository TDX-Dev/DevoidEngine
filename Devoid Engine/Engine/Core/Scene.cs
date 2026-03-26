using DevoidEngine.Engine.Components;

namespace DevoidEngine.Engine.Core
{
    public class Scene
    {
        public event Action<Component>? OnComponentAdded;
        public event Action<Component>? OnComponentRemoved;

        public List<GameObject> GameObjects { get; private set; }

        private bool isPlaying = false;
        private List<Transform3D> transforms;


        public Scene()
        {
            GameObjects = new List<GameObject>();
            transforms = new List<Transform3D>();
        }

        public void Play(bool value) => isPlaying = value;

        public void Update(float deltaTime)
        {
            if (!isPlaying) { return; }

            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObjects[i].OnUpdate(deltaTime);
            }
        }

        public void FixedUpdate(float deltaTime)
        {
            if (isPlaying) { return; }
            for (int i = 0; i < GameObjects.Count - 1; i++)
            {
                GameObjects[i].OnFixedUpdate(deltaTime);
            }
        }

        public GameObject AddGameObject(string name = "GameObject")
        {
            GameObject gameObject = new GameObject();
            gameObject.Name = name;
            Transform3D transform = gameObject.AddComponent<Transform3D>();
            gameObject.Transform = transform;
            GameObjects.Add(gameObject);
            transforms.Add(transform);
            return gameObject;
        }

        public void DestroyGameObject(GameObject gameObject)
        {
            transforms.Remove(gameObject.Transform);
            gameObject.OnDestroy();
            GameObjects.Remove(gameObject);
        }

        public List<T> GetComponentsOfType<T>() where T : Component
        {
            List<T> components = new List<T>();

            for (int i = 0; i < GameObjects.Count; i++)
            {
                var go = GameObjects[i];

                for (int j = 0; j < go.Components.Count; j++)
                {
                    if (go.Components[j] is T comp)
                    {
                        components.Add(comp);
                    }
                }
            }

            return components;
        }

        public void ComponentAdded(Component component)
        {
            OnComponentAdded?.Invoke(component);
        }

        public void ComponentRemoved(Component component)
        {
            OnComponentRemoved?.Invoke(component);
        }
    }
}
