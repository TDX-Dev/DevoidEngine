using DevoidEngine.Engine.ProjectSystem;
using System.Diagnostics;
using System.Text;

namespace ElementalEditor.Scripting;

public static class ScriptProjectGenerator
{
    public static void Generate()
    {
        var project = ProjectManager.Current!;
        string path = Path.Combine(project.RootPath, "GameScripts.csproj");
        string sln = Path.Combine(project.RootPath, "GameScripts.sln");

        if (!File.Exists(path))
        {
            string projectFileContent = GetProjectFileContent();
            File.WriteAllText(path, projectFileContent);
        }

        if (!File.Exists(sln))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "new sln -n GameScripts --force",
                WorkingDirectory = project.RootPath
            })?.WaitForExit();

            Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "sln GameScripts.sln add GameScripts.csproj",
                WorkingDirectory = project.RootPath
            })?.WaitForExit();
        }

        EnsureOutputDirectory(project);
    }

    static string GetProjectFileContent()
    {
        string engineDll = GetEngineDllPath();

        var sb = new StringBuilder();

        sb.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");

        sb.AppendLine("  <PropertyGroup>");
        sb.AppendLine("    <TargetFramework>net9.0</TargetFramework>");
        sb.AppendLine("    <OutputType>Library</OutputType>");
        sb.AppendLine("    <Nullable>enable</Nullable>");
        sb.AppendLine("    <ImplicitUsings>enable</ImplicitUsings>");

        sb.AppendLine("    <EnableDefaultItems>false</EnableDefaultItems>");

        sb.AppendLine("    <OutputPath>./Temp/Scripts/</OutputPath>");
        sb.AppendLine("    <BaseIntermediateOutputPath>Temp/obj/</BaseIntermediateOutputPath>");
        sb.AppendLine("    <IntermediateOutputPath>Temp/obj/</IntermediateOutputPath>");
        sb.AppendLine("    <BaseOutputPath>Temp/bin/</BaseOutputPath>");
        sb.AppendLine("    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>");
        sb.AppendLine("    <CopyLocalLockFileAssemblies>false</CopyLocalLockFileAssemblies>");
        sb.AppendLine("    <GenerateDependencyFile>false</GenerateDependencyFile>");
        sb.AppendLine("    <GenerateRuntimeConfigurationFiles>false</GenerateRuntimeConfigurationFiles>");

        sb.AppendLine("    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>");
        sb.AppendLine("    <CompilerGeneratedFilesOutputPath>Temp/Generated</CompilerGeneratedFilesOutputPath>");
        sb.AppendLine("  </PropertyGroup>");

        sb.AppendLine("  <ItemGroup>");
        sb.AppendLine("    <Compile Include=\"Assets/**/*.cs\" />");
        sb.AppendLine("  </ItemGroup>");

        sb.AppendLine("  <ItemGroup>");
        sb.AppendLine("    <None Include=\"Assets/**/*.*\" Exclude=\"Assets/**/*.cs\" />");
        sb.AppendLine("  </ItemGroup>");

        sb.AppendLine("  <ItemGroup>");
        sb.AppendLine("    <Folder Include=\"Assets/\" />");
        sb.AppendLine("  </ItemGroup>");

        sb.AppendLine("  <ItemGroup>");
        sb.AppendLine("    <Reference Include=\"DevoidEngine\">");
        sb.AppendLine($"      <HintPath>{engineDll}</HintPath>");
        sb.AppendLine("      <Private>false</Private>");
        sb.AppendLine("    </Reference>");
        sb.AppendLine("  </ItemGroup>");

        string sourceGenDll = Path.Combine(AppContext.BaseDirectory, "DevoidEngine.SourceGen.dll").Replace("\\", "/");
        sb.AppendLine("  <ItemGroup>");
        sb.AppendLine($"    <Analyzer Include=\"{sourceGenDll}\" OutputItemType=\"Analyzer\" ReferenceOutputAssembly=\"false\" />");
        sb.AppendLine("  </ItemGroup>");

        string messagePackDll = Path.Combine(AppContext.BaseDirectory, "MessagePack.dll").Replace("\\", "/");

        sb.AppendLine("  <ItemGroup>");
        sb.AppendLine("    <Reference Include=\"MessagePack\">");
        sb.AppendLine($"      <HintPath>{messagePackDll}</HintPath>");
        sb.AppendLine("      <Private>false</Private>");
        sb.AppendLine("    </Reference>");
        sb.AppendLine("  </ItemGroup>");

        sb.AppendLine("</Project>");
        return sb.ToString();
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