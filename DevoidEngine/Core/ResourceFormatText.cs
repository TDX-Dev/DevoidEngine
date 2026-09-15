using System.Text;

namespace DevoidEngine.Core
{
    public static class ResourceFormatText
    {
        public static void Save(string path, Scene scene)
        {
            StringBuilder sb = new();

            sb.AppendLine($"[dv_scene id={scene.Guid.ToString("N")}]");

            sb.AppendLine();

            for (int i = 0; i < scene.GameObjects.Count; i++)
            {
                GameObject go = scene.GameObjects[i];
                sb.AppendLine($"[gameobject name=\"{go.Name}\" id=\"{go.Id.ToString("N")}\" parent=\"{go.Parent?.Id.ToString("N") ?? Guid.Empty.ToString("N")}\"]");

                sb.AppendLine();
            }

            
            File.WriteAllText(Path.Join(Engine.Instance.ProjectSystem.AssetPath, path), sb.ToString());
        }


    }
}
