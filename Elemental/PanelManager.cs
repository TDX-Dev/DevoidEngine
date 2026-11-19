namespace Elemental
{
    public class PanelManager
    {
        private readonly List<Panel> _panels = [];
        private readonly List<Panel> _panelsToAdd = [];
        private readonly List<Panel> _panelsToRemove = [];

        public T AddPanel<T>(T panel) where T : Panel
        {
            _panelsToAdd.Add(panel);
            return panel;
        }

        public void RemovePanel(Panel panel)
        {
            _panelsToRemove.Add(panel);
        }

        public T? GetPanel<T>() where T : Panel
        {
            foreach (var panel in _panels)
            {
                if (panel is T typedPanel)
                    return typedPanel;
            }
            return null;
        }

        public void OnUpdate(EditorContext context, float deltaTime)
        {
            ProcessPendingChanges(context);

            for (int i = 0; i < _panels.Count; i++)
            {
                if (_panels[i].IsOpen)
                {
                    _panels[i].OnUpdate(context, deltaTime);
                }
            }
        }

        public void OnRender(EditorContext context)
        {
            for (int i = 0; i < _panels.Count; i++)
            {
                if (_panels[i].IsOpen)
                {
                    _panels[i].OnRender(context);
                }
            }
        }

        public void OnImGuiRender(EditorContext context)
        {
            for (int i = 0; i < _panels.Count; i++)
            {
                _panels[i].Draw(context);
            }
        }

        public void ProcessPendingChanges(EditorContext context)
        {
            if (_panelsToAdd.Count > 0)
            {
                foreach (var panel in _panelsToAdd)
                {
                    _panels.Add(panel);
                    panel.OnAttach(context);
                }
                _panelsToAdd.Clear();
            }

            if (_panelsToRemove.Count > 0)
            {
                foreach (var panel in _panelsToRemove)
                {
                    panel.OnDetach();
                    _panels.Remove(panel);
                }
                _panelsToRemove.Clear();
            }
        }

        public void Clear()
        {
            foreach (var panel in _panels)
            {
                panel.OnDetach();
            }
            _panels.Clear();
        }
    }
}