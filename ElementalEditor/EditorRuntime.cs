using DevoidEngine.Engine.ProjectSystem;
using ElementalEditor.Utils;
using System.Diagnostics;

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

            EnsureRuntimeProject();
            EnsureRuntimeBuild();

            string runtimeExe = Path.Combine(
                ProjectManager.Current.TempPath,
                "EditorRuntime",
                "DevoidRuntime.exe");

            if (!File.Exists(runtimeExe))
            {
                Console.WriteLine("[Runtime] Runtime executable not found.");
                return;
            }

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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Runtime] error: {ex.Message}");
            }
        }

        public static void Stop()
        {
            if (runtimeProcess != null && !runtimeProcess.HasExited)
                runtimeProcess.Kill();
        }

        static void EnsureRuntimeProject()
        {
            var project = ProjectManager.Current;

            string src = Path.Combine(
                AppContext.BaseDirectory,
                "DevoidRuntime");

            string dst = Path.Combine(
                project.TempPath,
                "DevoidRuntime");

            Console.WriteLine("[Runtime] Source runtime dir: " + src);
            Console.WriteLine("[Runtime] Destination runtime dir: " + dst);

            if (!File.Exists(Path.Combine(dst, "DevoidRuntime.csproj")))
            {
                Console.WriteLine("[Runtime] Copying runtime template...");
                FileSystemUtil.CopyDirectory(src, dst);
            }
        }

        static void EnsureRuntimeBuild()
        {
            var project = ProjectManager.Current;

            string runtimeExe = Path.Combine(
                project.TempPath,
                "EditorRuntime",
                "DevoidRuntime.exe");

            if (File.Exists(runtimeExe))
                return;

            Console.WriteLine("[Runtime] Building editor runtime...");

            CopyRuntimeDependencies();
            BuildRuntime();
        }

        static void BuildRuntime()
        {
            var project = ProjectManager.Current;

            string runtimeProject = Path.Combine(
                project.TempPath,
                "DevoidRuntime",
                "DevoidRuntime.csproj");

            string outputDir = Path.Combine(
                project.TempPath,
                "EditorRuntime");

            Directory.CreateDirectory(outputDir);

            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments =
                    $"publish \"{runtimeProject}\" " +
                    "-c Debug " +
                    "-p:RuntimeEmbed=true " +
                    "-p:PublishAot=false " +
                    "-p:DefineConstants=SCRIPT_DYNAMIC " +
                    $"-o \"{outputDir}\"",
                WorkingDirectory = Path.GetDirectoryName(runtimeProject),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var process = new Process();
            process.StartInfo = psi;

            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null)
                    Console.WriteLine("[Runtime Build] " + e.Data);
            };

            process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null)
                    Console.WriteLine("[Runtime Build ERROR] " + e.Data);
            };

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();

            string exe = Path.Combine(outputDir, "DevoidRuntime.exe");

            if (!File.Exists(exe))
                throw new Exception("Runtime build failed. See console for errors.");
        }

        static void CopyRuntimeDependencies()
        {
            var project = ProjectManager.Current;

            string srcDir = AppContext.BaseDirectory;

            string editorRuntime = Path.Combine(
                project.TempPath,
                "EditorRuntime");

            string runtimeProject = Path.Combine(
                project.TempPath,
                "DevoidRuntime");

            Directory.CreateDirectory(editorRuntime);

            string[] files =
            {
        "DevoidEngine.dll",
        "DevoidGPU.dll"
    };

            foreach (var f in files)
            {
                string src = Path.Combine(srcDir, f);

                if (!File.Exists(src))
                    continue;

                File.Copy(src, Path.Combine(editorRuntime, f), true);
                File.Copy(src, Path.Combine(runtimeProject, f), true);
            }
        }
    }
}