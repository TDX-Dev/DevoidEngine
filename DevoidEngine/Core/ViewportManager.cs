using DevoidEngine.Rendering;
using DevoidGPU;

namespace DevoidEngine.Core
{
    public enum ViewportPriority
    {
        PrePass = 0,    // SubViewports (e.g., CRT TV, Minimap, Portals, Offscreen Previews)
        Main = 100,     // Main Game Views & Editor Scene Views
        PostPass = 200  // UI / Overlay / Canvas Viewports
    }

    public class ViewportManager
    {
        private class ViewportEntry
        {
            public Viewport Viewport { get; set; } = null!;
            public ViewportPriority Priority { get; set; }
        }

        private readonly List<ViewportEntry> _entries = [];
        private readonly List<Viewport> _cachedSortedViewports = [];
        private bool _isDirty = false;

        /// <summary>
        /// Registers a viewport to be rendered.
        /// </summary>
        public void RegisterViewport(Viewport viewport, ViewportPriority priority = ViewportPriority.Main)
        {
            if (viewport == null || Contains(viewport))
                return;

            _entries.Add(new ViewportEntry
            {
                Viewport = viewport,
                Priority = priority
            });


            _isDirty = true;
        }

        /// <summary>
        /// Unregisters a viewport so it won't be updated or rendered.
        /// </summary>
        public void UnregisterViewport(Viewport viewport)
        {
            if (viewport == null) return;

            int countBefore = _entries.Count;
            _entries.RemoveAll(e => e.Viewport == viewport);

            if (_entries.Count != countBefore)
            {
                _isDirty = true;
            }
        }

        public bool Contains(Viewport viewport)
        {
            return _entries.Exists(e => e.Viewport == viewport);
        }

        public List<Viewport> GetViewports()
        {
            if (_isDirty)
            {
                SortEntries();
            }

            return _cachedSortedViewports;
        }

        public void RenderAll(ICommandList cmd)
        {
            var viewports = GetViewports();

            for (int i = 0; i < viewports.Count; i++)
            {
                Viewport viewport = viewports[i];

                if (viewport.TargetScene == null || viewport.ActiveCamera == null || !viewport.CanRender)
                    continue;
                Engine.Renderer.RenderViewport(cmd, viewport);
            }
        }

        private void SortEntries()
        {
            _entries.Sort((a, b) => a.Priority.CompareTo(b.Priority));

            _cachedSortedViewports.Clear();
            for (int i = 0; i < _entries.Count; i++)
            {
                _cachedSortedViewports.Add(_entries[i].Viewport);
            }

            _isDirty = false;
        }

        public void Clear()
        {
            _entries.Clear();
            _cachedSortedViewports.Clear();
            _isDirty = false;
        }
    }
}