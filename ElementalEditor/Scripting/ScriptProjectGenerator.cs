using System.Text;
using DevoidEngine.Engine.ProjectSystem;

namespace ElementalEditor.Scripting;

public static class ScriptProjectGenerator
{
    public static void Generate()
    {
        var project = ProjectManager.Current!;
        string path = Path.Combine(project.SettingsPath, "GameScripts.csproj");

        if (File.Exists(path))
            return;

        Directory.CreateDirectory(project.SettingsPath);

        string engineDll = GetEngineDllPath();

        var sb = new StringBuilder();

        sb.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");

        sb.AppendLine("  <PropertyGroup>");
        sb.AppendLine("    <TargetFramework>net9.0</TargetFramework>");
        sb.AppendLine("    <OutputType>Library</OutputType>");
        sb.AppendLine("    <Nullable>enable</Nullable>");
        sb.AppendLine("    <ImplicitUsings>enable</ImplicitUsings>");

        sb.AppendLine("    <OutputPath>../Temp/Scripts/</OutputPath>");
        sb.AppendLine("    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>");
        sb.AppendLine("    <CopyLocalLockFileAssemblies>false</CopyLocalLockFileAssemblies>");
        sb.AppendLine("    <GenerateDependencyFile>false</GenerateDependencyFile>");
        sb.AppendLine("    <GenerateRuntimeConfigurationFiles>false</GenerateRuntimeConfigurationFiles>");

        sb.AppendLine("    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>");
        sb.AppendLine("    <CompilerGeneratedFilesOutputPath>../Temp/Generated</CompilerGeneratedFilesOutputPath>");
        sb.AppendLine("  </PropertyGroup>");

        sb.AppendLine("  <ItemGroup>");
        sb.AppendLine("    <Compile Include=\"../Assets/**/*.cs\" />");
        sb.AppendLine("  </ItemGroup>");

        sb.AppendLine("  <ItemGroup>");
        sb.AppendLine("    <Reference Include=\"DevoidEngine\">");
        sb.AppendLine($"      <HintPath>{engineDll}</HintPath>");
        sb.AppendLine("      <Private>false</Private>");
        sb.AppendLine("    </Reference>");
        sb.AppendLine("  </ItemGroup>");

        string sourceGenDll = Path.Combine(AppContext.BaseDirectory, "DevoidEngine.SourceGen.dll");

        sb.AppendLine("  <ItemGroup>");
        sb.AppendLine($"    <Analyzer Include=\"{sourceGenDll}\" />");
        sb.AppendLine("  </ItemGroup>");

        string messagePackDll = Path.Combine(AppContext.BaseDirectory, "MessagePack.dll").Replace("\\", "/");

        sb.AppendLine("  <ItemGroup>");
        sb.AppendLine($"    <Reference Include=\"MessagePack\">");
        sb.AppendLine($"      <HintPath>{messagePackDll}</HintPath>");
        sb.AppendLine("      <Private>false</Private>");
        sb.AppendLine("    </Reference>");
        sb.AppendLine("  </ItemGroup>");

        sb.AppendLine("</Project>");

        File.WriteAllText(path, sb.ToString());

        EnsureOutputDirectory(project);
    }

    static void EnsureOutputDirectory(Project project)
    {
        string scriptsDir = Path.Combine(project.TempPath, "Scripts");
        Directory.CreateDirectory(scriptsDir);
    }

    static string GetEngineDllPath()
    {
        string exeDir = AppContext.BaseDirectory;

        return Path.Combine(exeDir, "DevoidEngine.dll");
    }
}