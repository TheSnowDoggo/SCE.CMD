namespace SCE
{
    internal static class Launch
    {
        internal static void Main()
        {
            CmdLauncher launcher = new($"- SCE Launcher v{CmdLauncher.VERSION} -");

            launcher.SLoadPackages(new Package[]
            {
                new NativePKG(),
                new MemoryPKG(),
                new ConsolePKG(),
                new ExternalPKG(),
                new AliasPKG(),
                new VariablePKG(),
                new CombinePKG(),
                new DefinePKG(),
                new StatePKG(),
            });

            var scrPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "autoscripts");
            if (Directory.Exists(scrPath))
                launcher.SExecuteCommand("scrrundir", new[] { scrPath });
            else
                Directory.CreateDirectory(scrPath);

            Console.WriteLine($"{launcher.Name}\nStart typing or type help to see available commands:");
            launcher.Run();
        }
    }
}