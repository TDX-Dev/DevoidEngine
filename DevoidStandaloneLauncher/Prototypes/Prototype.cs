using DevoidEngine.Engine.Core;

namespace DevoidStandaloneLauncher.Prototypes
{
    internal class Prototype
    {
        public BaseGame baseLayer;
        public virtual void OnInit(Scene main) { }
        public virtual void OnUpdate(float delta) { }
        public virtual void OnRender(float delta) { }

    }
}
