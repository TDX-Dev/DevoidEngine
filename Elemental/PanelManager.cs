namespace Elemental
{
    public class PanelManager
    {
        private readonly List<Panel> panels = [];
        private readonly List<Panel> panelsToAdd = [];
        private readonly List<Panel> panelsToRemove = [];

        public T AddPanel<T>(T panel) where T : Panel
        {
            panelsToAdd.Add(panel);
            return panel;
        }

        public void RemovePanel(Panel panel)
        {
            panelsToRemove.Add(panel);
        }

        public T? GetPanel<T>() where T : Panel
        {
            foreach (var panel in panels)
            {
                if (panel is T typedPanel)
                    return typedPanel;
            }
            return null;
        }

        public void OnUpdate(float deltaTime)
        {
            ProcessPendingChanges();

            for (int i = 0; i < panels.Count; i++)
            {
                if (panels[i].IsOpen)
                {
                    panels[i].OnUpdate(deltaTime);
                }
            }
        }

        public void OnImGuiRender()
        {
            for (int i = 0; i < panels.Count; i++)
            {
                panels[i].Draw();
            }
        }

        private void ProcessPendingChanges()
        {
            if (panelsToAdd.Count > 0)
            {
                foreach (var panel in panelsToAdd)
                {
                    panels.Add(panel);
                    panel.OnAttach();
                }
                panelsToAdd.Clear();
            }

            if (panelsToRemove.Count > 0)
            {
                foreach (var panel in panelsToRemove)
                {
                    panel.OnDetach();
                    panels.Remove(panel);
                }
                panelsToRemove.Clear();
            }
        }

        public void Clear()
        {
            foreach (var panel in panels)
            {
                panel.OnDetach();
            }
            panels.Clear();
        }
    }
}