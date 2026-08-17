using DevoidEngine.Core;
using DevoidEngine.Rendering;
using DevoidGPU;
using System.Collections.Generic;

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

        private readonly List<ViewportEntry> entries = [];
        private readonly List<Viewport> cachedSortedViewports = [];
        private bool isDirty = false;

        /// <summary>
        /// Registers a viewport to be rendered.
        /// </summary>
        public void RegisterViewport(Viewport viewport, ViewportPriority priority = ViewportPriority.Main)
        {
            if (viewport == null || Contains(viewport))
                return;

            entries.Add(new ViewportEntry
            {
                Viewport = viewport,
                Priority = priority
            });

            isDirty = true;
        }

        /// <summary>
        /// Unregisters a viewport so it won't be updated or rendered.
        /// </summary>
        public void UnregisterViewport(Viewport viewport)
        {
            if (viewport == null) return;

            int countBefore = entries.Count;
            entries.RemoveAll(e => e.Viewport == viewport);

            if (entries.Count != countBefore)
            {
                isDirty = true;
            }
        }

        public bool Contains(Viewport viewport)
        {
            return entries.Exists(e => e.Viewport == viewport);
        }

        public List<Viewport> GetViewports()
        {
            if (isDirty)
            {
                SortEntries();
            }

            return cachedSortedViewports;
        }

        public void RenderAll(ICommandList cmd)
        {
            var viewports = GetViewports();

            for (int i = 0; i < viewports.Count; i++)
            {
                Viewport viewport = viewports[i];


                if (viewport.TargetScene == null || viewport.ActiveCamera == null || !viewport.Render)
                    continue;

                // Calls your existing Renderer loop
                Engine.Renderer.Render(cmd, viewport);
            }
        }

        private void SortEntries()
        {
            entries.Sort((a, b) => a.Priority.CompareTo(b.Priority));

            cachedSortedViewports.Clear();
            for (int i = 0; i < entries.Count; i++)
            {
                cachedSortedViewports.Add(entries[i].Viewport);
            }

            isDirty = false;
        }

        public void Clear()
        {
            entries.Clear();
            cachedSortedViewports.Clear();
            isDirty = false;
        }
    }
}