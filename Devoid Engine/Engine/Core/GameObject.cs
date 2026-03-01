using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Physics;

namespace DevoidEngine.Engine.Core
{
    public class GameObject
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name;

        public bool Enabled = true;

        public Transform transform { get; set; }

        private Scene scene;

        public GameObject parentObject;
        public List<GameObject> children;

        public List<Component> Components;

        private bool DestroyOnLoad = true;

        public Scene Scene
        {
            get { return scene; }
            set
            {
                scene = value;
                SetScene(value);
            }
        }

        public GameObject()
        {
            children = new List<GameObject>();
            Components = new List<Component>();

            // This is because the scene object doesnt get assigned to the game object on initialization (obviously!)
            // So to prevent errors on calling ComponentAdded from scene, this bandaid solution has been implemented.
        }

        public void Initialize()
        {
            transform = new Transform();
            transform.gameObject = this;
            Components.Add(transform);


        }

        public T AddComponent<T>() where T : Component, new()
        {
            T _component = new();
            _component.gameObject = this;
            Components.Add(_component);
            scene?.ComponentAdded(_component);
            return _component;
        }

        public Component AddComponent(Component component)
        {
            Component _component = component;
            _component.gameObject = this;
            Components.Add(_component);
            scene?.ComponentAdded(_component);
            return _component;
        }

        public T GetComponent<T>() where T : Component, new()
        {
            for (int i = 0; i < Components.Count; i++)
            {
                if (typeof(T) == Components[i].GetType())
                {
                    return (T)Components[i];
                }
            }
            return null;
        }
        public Component GetComponent(Type type)
        {
            foreach (var comp in Components)
            {
                if (type.IsAssignableFrom(comp.GetType()))
                {
                    return comp;
                }
            }
            return null;
        }

        public void RemoveComponent<T>() where T : Component
        {
            for (int i = 0; i < Components.Count; i++)
            {
                if (typeof(T) == Components[i].GetType())
                {
                    scene?.ComponentRemoved(Components[i]);
                    Components[i].OnDestroy();
                    Components.RemoveAt(i);
                }
            }
        }

        public void RemoveComponent(Component component)
        {
            if (Components.Contains(component))
            {
                scene?.ComponentRemoved(component);
                component.OnDestroy();
                Components.Remove(component);
            }

        }

        public void SetScene(Scene scene)
        {
            if (children.Count == 0) { return; }

            for (int i = 0; i < children.Count; i++)
            {
                children[i].Scene = scene;
            }
        }

        public List<Component> GetComponents()
        {
            return Components;
        }

        public List<Component> GetComponentsRecursive()
        {
            List<Component> result = new List<Component>();
            CollectComponentsRecursive(this, result);
            return result;
        }

        private void CollectComponentsRecursive(GameObject obj, List<Component> list)
        {
            list.AddRange(obj.Components);

            foreach (var child in obj.children)
            {
                CollectComponentsRecursive(child, list);
            }
        }


        public void AddChild(GameObject child, bool keepWorld = true)
        {
            if (child == null || child == this)
                return;

            child.SetParent(this, keepWorld);
        }

        public void SetParent(GameObject newParent, bool keepWorld = true)
        {
            if (parentObject == newParent)
                return;

            // Remove from old parent list
            if (parentObject != null)
                parentObject.children.Remove(this);

            GameObject current = newParent;
            while (current != null)
            {
                if (current == this)
                    return; // prevent circular parenting
                current = current.parentObject;
            }

            parentObject = newParent;

            if (newParent != null)
            {
                if (!newParent.children.Contains(this))
                    newParent.children.Add(this);
            }
            transform.SetParent(newParent?.transform, keepWorld);
        }


        public void OnStart()
        {

            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].InternalStart();
            }

            //if (children.Count > 0)
            //{
            //    for (int i = 0; i < children.Count; i++)
            //    {
            //        children[i].OnStart();
            //    }
            //}
        }

        public void OnUpdate(float dt)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].OnUpdate(dt);
            }



            //if (children.Count > 0)
            //{
            //    for (int i = 0; i < children.Count; i++)
            //    {
            //        children[i].OnUpdate(dt);
            //    }
            //}
        }

        public void OnFixedUpdate(float dt)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].OnFixedUpdate(dt);
            }

            foreach (var child in children)
                child.OnFixedUpdate(dt);
        }

        public void OnLateUpdate(float dt)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].OnLateUpdate(dt);
            }

            foreach (var child in children)
                child.OnLateUpdate(dt);
        }


        public void OnRender(float dt)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].OnRender(dt);
            }



            //if (children.Count > 0)
            //{
            //    for (int i = 0; i < children.Count; i++)
            //    {
            //        children[i].OnRender(dt);
            //    }
            //}
        }

        internal void InvokeCollisionEnter(GameObject other)
        {
            var snapshot = new List<Component>(Components);

            foreach (var comp in snapshot)
            {
                if (comp is ICollisionListener listener)
                    listener.OnCollisionEnter(other);
            }
        }

        internal void InvokeCollisionStay(GameObject other)
        {
            var snapshot = new List<Component>(Components);

            foreach (var comp in snapshot)
            {
                if (comp is ICollisionListener listener)
                    listener.OnCollisionStay(other);
            }
        }

        internal void InvokeCollisionExit(GameObject other)
        {
            var snapshot = new List<Component>(Components);

            foreach (var comp in snapshot)
            {
                if (comp is ICollisionListener listener)
                    listener.OnCollisionExit(other);
            }
        }


        // This method is called when the objects are destroyed when another scene is loaded
        public bool OnDestroyLoad()
        {
            if (DestroyOnLoad)
            {
                for (int i = 0; i < Components.Count; i++)
                {
                    Components[i].OnDestroy();
                }
            }
            return DestroyOnLoad;
        }

        // This is called when the object is requested to be removed
        public void OnDestroy()
        {
            var comps = new List<Component>(Components);

            foreach (var comp in comps)
                RemoveComponent(comp);

            foreach (var child in children)
                child.OnDestroy();
        }

    }
}
