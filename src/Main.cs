namespace SCE
{
    internal static class Launch
    {
        internal static void Main()
        {
            CmdLauncher launcher = new()
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
                    new VariablePKG(),
                    new ToolsPKG(),
                },
            };

            launcher.Run();
        }
    }
}