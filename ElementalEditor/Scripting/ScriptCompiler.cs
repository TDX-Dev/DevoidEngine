using DevoidEngine.Engine.ProjectSystem;
using System.Diagnostics;

namespace ElementalEditor.Scripting
{
    public static class ScriptCompiler
    {

        public static bool Compile(out string errors)
        {
            var project = ProjectManager.Current!;

            string csproj = Path.Combine(
                project.RootPath,
                "GameScripts.csproj"
            );

            ProcessStartInfo psi = new()
            {
                FileName = "dotnet",
                Arguments = $"build \"{csproj}\" --no-restore -c Debug",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(psi)!;

            string output = process.StandardOutput.ReadToEnd();
            string err = process.StandardError.ReadToEnd();

            process.WaitForExit();

            errors = /*output + "\n" +*/ err;

            return process.ExitCode == 0;
        }

    }
}
