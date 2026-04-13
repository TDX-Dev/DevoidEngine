#if SCRIPT_STATIC
static class StaticScriptLoader
{
    public static void Load()
    {
        GameScriptRegistry.RegisterAll();
    }
}
#endif