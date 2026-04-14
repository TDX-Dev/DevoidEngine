using System.Reflection;
using System.Runtime.Loader;

namespace ElementalEditor.Scripting;

class ScriptLoadContext : AssemblyLoadContext
{
    public ScriptLoadContext() : base(isCollectible: true) { }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        return null;
    }

    ~ScriptLoadContext()
    {
        Console.WriteLine("ScriptLoadContext finalized");
    }
}