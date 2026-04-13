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
            CopyRuntimeDependencies();

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
            BuildRuntime();
        }

        static void BuildRuntime()
        {
            var project = ProjectManager.Current;

            string runtimeProject = Path.Combine(
                AppContext.BaseDirectory,
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
                throw new Exception("Runtime build failed.");
        }

        static void CopyRuntimeDependencies()
        {
            var project = ProjectManager.Current;

            string srcDir = AppContext.BaseDirectory;
            string dstDir = Path.Combine(project.TempPath, "EditorRuntime");

            Directory.CreateDirectory(dstDir);

            // copy all dlls beside editor
            foreach (var file in Directory.GetFiles(srcDir, "*.dll"))
            {
                string name = Path.GetFileName(file);

                if (name.StartsWith("ElementalEditor"))
                    continue;

                File.Copy(file, Path.Combine(dstDir, name), true);
            }

            // copy native runtime libraries directly beside runtime exe
            string nativeDir = Path.Combine(srcDir, "runtimes", "win-x64", "native");

            if (Directory.Exists(nativeDir))
            {
                foreach (var dll in Directory.GetFiles(nativeDir, "*.dll"))
                {
                    File.Copy(dll, Path.Combine(dstDir, Path.GetFileName(dll)), true);
                }
            }
        }
    }
}