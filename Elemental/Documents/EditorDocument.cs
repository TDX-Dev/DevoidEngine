namespace Elemental.Documents
{
    public abstract class EditorDocument
    {
        public string? FilePath { get; private set; }

        public bool IsDirty { get; private set; }

        public bool HasFile =>
            !string.IsNullOrWhiteSpace(FilePath);

        public bool HasUnsavedChanges =>
            IsDirty;

        public virtual string DisplayName => FilePath != null ? Path.GetFileNameWithoutExtension(FilePath) : "Untitled";

        protected EditorDocument(string? filePath = null)
        {
            FilePath = filePath;
        }

        public void MarkDirty()
        {
            IsDirty = true;
        }

        public void MarkSaved(string path)
        {
            FilePath = Path.GetFullPath(path);
            IsDirty = false;
        }

        public void MarkLoaded(string path)
        {
            FilePath = Path.GetFullPath(path);
            IsDirty = false;
        }

        public void MarkUnsaved()
        {
            FilePath = null;
            IsDirty = true;
        }
    }
}