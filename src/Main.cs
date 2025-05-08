namespace SCE
{
    internal static class Launch
    {
        internal static void Main()
        {
            CmdLauncher launcher = new("- SCE Launcher v0.5.2 -");

            launcher.SafeLoadPackages(new Package[]
            {
                new NativePKG(),
                new MemoryPKG(),
                new ConsolePKG(),
                new ExternalPKG(),
                new AliasPKG(),
                new VariablePKG(),
                new CombinePKG(),
                new ToolsPKG(),
            });

            var scrPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "autoscripts");
            if (Directory.Exists(scrPath))
                launcher.SExecuteCommand("scrrundir", new[] { scrPath });
            else
                Directory.CreateDirectory(scrPath);

            launcher.Run();
        }
    }
}