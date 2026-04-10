using DevoidEngine.Engine.ProjectSystem;
using System.Diagnostics;
using System.IO;

namespace ElementalEditor
{
    public static class EditorRuntime
    {
        static Process runtimeProcess;

        public static bool IsRunning
        {
            get
            {
                if (runtimeProcess == null)
                    return false;

                if (runtimeProcess.HasExited)
                {
                    runtimeProcess.Dispose();
                    runtimeProcess = null;
                    return false;
                }

                return true;
            }
        }

        public static void Launch()
        {
            if (IsRunning)
                return;

            string runtimeExe = Path.Combine(AppContext.BaseDirectory, "DevoidRuntime.exe");

            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = runtimeExe,
                Arguments = $"--project \"{ProjectManager.Current.ProjectFile}\"",
                UseShellExecute = true
            };

            runtimeProcess = Process.Start(start);
        }

        public static void Stop()
        {
            if (runtimeProcess != null && !runtimeProcess.HasExited)
                runtimeProcess.Kill();
        }
    }
}