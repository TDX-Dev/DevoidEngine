using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        /// <summary> Called when the panel is first added to the Editor. </summary>
        public virtual void OnAttach() { }

        /// <summary> Called when the panel is removed or shut down. </summary>
        public virtual void OnDetach() { }

        /// <summary> Frame update loop (e.g., updating controllers, animations, rays). </summary>
        public virtual void OnUpdate(float deltaTime) { }

        /// <summary> Called inside the ImGui render pass. </summary>
        public void Draw()
        {
            if (!IsOpen)
                return;

            bool visible = OnBeginWindow();

            if (visible)
                OnImGuiRender();

            OnEndWindow();
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

        protected abstract void OnImGuiRender();

        protected virtual void OnEndWindow()
        {
            ImGui.End();
        }
    }
}
