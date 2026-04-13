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

        public static void Launch(string? scenePath = null)
        {
            if (IsRunning)
                return;

            string runtimeExe = Path.Combine(AppContext.BaseDirectory, "DevoidRuntime.exe");

            string args =
                $"--project \"{ProjectManager.Current.ProjectFile}\" --mode editor";

            if (!string.IsNullOrEmpty(scenePath))
                args += $" --scene \"{scenePath}\"";

            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = runtimeExe,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            runtimeProcess = new Process();
            runtimeProcess.StartInfo = start;

            runtimeProcess.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null)
                    Console.WriteLine($"[Runtime] {e.Data}");
            };

            runtimeProcess.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null)
                    Console.WriteLine($"[Runtime ERROR] {e.Data}");
            };

            try
            {
                runtimeProcess.Start();
                runtimeProcess.BeginOutputReadLine();
                runtimeProcess.BeginErrorReadLine();
            } catch (Exception ex)
            {
                Console.WriteLine($"[Runtime] error: {ex.Message}");
            }
        }

        public static void Stop()
        {
            if (runtimeProcess != null && !runtimeProcess.HasExited)
                runtimeProcess.Kill();
        }
    }
}