namespace CMD
{
    internal static class Launcher
    {
        internal static void Main()
        {
            CommandLauncher launcher = new()
            {
                Name = "- SCE Launcher -",
                Packages = new()
                {
                    new ConsolePKG(),
                    new PrintPKG(),
                    new EvaluatorPKG(),
                    new ScriptPKG(),
                    new AliasPKG(),
                    new PrimePKG(),
                    new StoragePKG(),
                },
            };

            launcher.Run();
        }
    }
}