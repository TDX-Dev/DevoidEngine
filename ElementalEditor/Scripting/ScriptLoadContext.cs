using System.Reflection;
using System.Runtime.Loader;

namespace ElementalEditor.Scripting;

public class ScriptLoadContext : AssemblyLoadContext
{
    public ScriptLoadContext() : base("GameScriptsContext", isCollectible: true)
    {
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // Let default context resolve engine dependencies
        return null;
    }
}