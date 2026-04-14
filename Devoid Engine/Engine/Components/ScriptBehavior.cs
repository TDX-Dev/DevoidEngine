using DevoidEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Components
{
    public abstract class ScriptBehaviour
    {
        public GameObject gameObject = null!;

        public virtual void OnStart() { }
        public virtual void OnUpdate(float dt) { }
        public virtual void OnLateUpdate(float dt) { }
        public virtual void OnFixedUpdate(float dt) { }
        public virtual void OnRender() { }
        public virtual void OnDestroy() { }
    }
}
