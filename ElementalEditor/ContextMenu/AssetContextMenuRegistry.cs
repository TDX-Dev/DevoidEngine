using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.ContextMenu
{
    public static class AssetContextMenuRegistry
    {
        class Entry
        {
            public string Extension;
            public Action<string> Draw;
        }

        static List<Entry> entries = new();

        public static void Register(string extension, Action<string> draw)
        {
            entries.Add(new Entry
            {
                Extension = extension.ToLower(),
                Draw = draw
            });
        }

        public static void Draw(string path)
        {
            string ext = Path.GetExtension(path).ToLower();

            foreach (var e in entries)
            {
                if (e.Extension == "*" || e.Extension == ext)
                    e.Draw(path);
            }
        }
    }
}
