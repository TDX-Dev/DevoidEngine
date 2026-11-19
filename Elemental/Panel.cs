using ImGuiNET;

namespace Elemental
{
    public abstract class Panel
    {
        public string Title { get; set; }
        public bool IsOpen { get => _isOpen; set => _isOpen = value; }
        public ImGuiWindowFlags WindowFlags { get; protected set; } = ImGuiWindowFlags.None;

        private bool _isOpen = true;
        private bool _focusRequested = false;


        protected Panel(string title)
        {
            Title = title;
        }

        public virtual void OnAttach(EditorContext context) { }
        public virtual void OnDetach() { }
        public virtual void OnUpdate(EditorContext context, float deltaTime) { }
        public virtual void OnRender(EditorContext context) { }
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
            _focusRequested = true;
        }
        protected virtual bool OnBeginWindow()
        {
            if (_focusRequested)
            {
                ImGui.SetNextWindowFocus();
                _focusRequested = false;
            }
            return ImGui.Begin(Title, ref _isOpen, WindowFlags);
        }

        protected abstract void OnImGuiRender(EditorContext context);
        protected virtual void OnEndWindow()
        {
            ImGui.End();
        }
    }
}