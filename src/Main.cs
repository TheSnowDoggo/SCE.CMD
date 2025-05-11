namespace SCE
{
    internal static class Launch
    {
        private static readonly Package[] _native = new Package[]
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
        };

        internal static void Main()
        {
            CmdLauncher cl = new($"- SCE Launcher v{CmdLauncher.VERSION} -");

            cl.SLoadPackages(_native);

            var scrPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "autoscripts");
            if (Directory.Exists(scrPath))
                cl.SExecuteCommand("scrrundir", new[] { scrPath });
            else
                Directory.CreateDirectory(scrPath);

            Console.WriteLine($"{cl.Name}\nStart typing or type help to see available commands:");
            cl.Run();
        }
    }
}