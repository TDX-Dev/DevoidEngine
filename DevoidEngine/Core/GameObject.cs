using DevoidEngine.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DevoidEngine.Core
{
    public class GameObject
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Scene Scene
        {
            get { return scene; }
            set
            {
                scene = value;
                SetScene(value);
            }
        }

        public Transform3D Transform { get; set; } = null!;

        public GameObject? Parent;
        public List<GameObject> Children;

        public List<Component> Components;

        public string Name;
        public bool Active = true;

        public override string ToString() => $"GameObject {Name}";

        private Scene scene = null!;

        public GameObject(string name = "")
        {
            Name = name;

            Transform = new()
            {
                gameObject = this
            };

            Children = [];
            Components = [Transform];
        }

        public void OnStart()
        {
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].InternalStart();
            }
        }

        public void OnUpdate(float dt)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].OnUpdate(dt);
            }
        }

        public void OnFixedUpdate(float dt)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].OnFixedUpdate(dt);
            }
        }
        
        public void OnRender()
        {
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].OnRender();
            }
        }
        public T AddComponent<T>() where T : Component, new()
        {
            T _component = new()
            {
                gameObject = this
            };
            Components.Add(_component);
            scene?.ComponentAdded(_component);
            _component.OnAttach();
            return _component;
        }
        public void RemoveComponent(Component component)
        {
            if (Components.Contains(component))
            {
                //scene?.ComponentRemoved(component);
                component.OnDestroy();
                Components.Remove(component);
            }

        }

        public void OnDestroy()
        {
            var comps = new List<Component>(Components);

            foreach (var comp in comps)
            {
                RemoveComponent(comp);
            }

            foreach (var child in Children)
                child.OnDestroy();
        }

        public void SetScene(Scene scene)
        {
            if (Children.Count == 0) { return; }

            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].Scene = scene;
            }
        }
    }
}
