namespace DevoidStandaloneLauncher.Prototypes
{
    public class Prototype
    {
        public PrototypeLoader loader;
        public virtual void OnInit() { }
        public virtual void OnUpdate(float delta) { }
        public virtual void OnFixedUpdate(float delta) { }
        public virtual void OnRender() { }
        public virtual void OnPostRender() { }
        public virtual void Resize(int width, int height) { }
    }
}