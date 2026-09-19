using ImGuiNET;

namespace Elemental
{
    public abstract class Panel
    {
        public string Title { get; set; }
        public bool IsOpen { get => isOpen; set => isOpen = value; }
        public ImGuiWindowFlags WindowFlags { get; protected set; } = ImGuiWindowFlags.None;

        private bool isOpen = true;
        private bool focusRequested = false;


        protected Panel(string title)
        {
            Title = title;
        }

        public virtual void OnAttach(EditorContext context) { }
        public virtual void OnDetach() { }
        public virtual void OnUpdate(EditorContext context, float deltaTime) { }
        protected virtual void OnPushStyle() { }

        protected virtual void OnPopStyle()
        {
        }
        public void Draw(EditorContext context)
        {
            if (!IsOpen)
                return;

            OnPushStyle();

            bool visible = OnBeginWindow();

            if (visible)
                OnImGuiRender(context);

            OnEndWindow();

            OnPopStyle();
        }

        public void RequestFocus()
        {
            focusRequested = true;
        }
        protected virtual bool OnBeginWindow()
        {
            if (focusRequested)
            {
                ImGui.SetNextWindowFocus();
                focusRequested = false;
            }
            return ImGui.Begin(Title, ref isOpen, WindowFlags);
        }

        protected abstract void OnImGuiRender(EditorContext context);
        protected virtual void OnEndWindow()
        {
            ImGui.End();
        }
    }
}