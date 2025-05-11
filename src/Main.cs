using CSUtils;
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
            try
            {
                CmdLauncher cl = new() { Version = new(0, 10, 5) };

                cl.SLoadPackages(_native);

                var scrPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "autoscripts");
                if (Directory.Exists(scrPath))
                    cl.SExecuteCommand("scrrundir", new[] { scrPath });
                else
                    Directory.CreateDirectory(scrPath);

                Console.WriteLine($"- SCE Launcher v{cl.Version} -\n" +
                    $"Start typing or type help to see available commands:");
                cl.Run();
            }
            catch (Exception e)
            {
                StrUtils.PrettyErr("Main", "An exception was thrown during launcher initialization:");
                Console.WriteLine(e.Message);
                if (Utils.BoolPrompt("Would you like to view the full error? Yes[Y] or No[N]: "))
                    Console.WriteLine(e);
                Utils.ContinueAny();
            }
        }
    }
}