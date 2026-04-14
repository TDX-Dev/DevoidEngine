#if SCRIPT_STATIC
static class StaticScriptLoader
{
    public static void Load()
    {
        Console.WriteLine("Statically loaded script");
        GameScriptRegistry.RegisterAll();
    }
}
#endif