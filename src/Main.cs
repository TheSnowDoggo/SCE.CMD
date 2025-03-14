using System.Reflection;
namespace SCE
{
    internal static class Launch
    {
        internal static void Main()
        {
            var launcher = NewLauncher();

            string aScripts = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "autoscripts");
            if (Directory.Exists(aScripts))
                launcher.SExecuteCommand("scrrundir", new[] { aScripts });
            else
                Directory.CreateDirectory(aScripts);

            launcher.Run();
        }

        internal static CmdLauncher NewLauncher()
        {
            return new()
            {
                Name = "- SCE Launcher -",
                Packages = new()
                {
                    new ConsolePKG(),
                    new PrintPKG(),
                    new EvaluatorPKG(),
                    new ExternalPKG(),
                    new AliasPKG(),
                    new PrimePKG(),
                    new VariablePKG(),
                    new ToolsPKG(),
                    new CombinePKG(),
                },
            };
        }
    }
}