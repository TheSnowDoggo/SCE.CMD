namespace SCE
{
    internal static class Launch
    {
        internal static void Main()
        {        
            CmdLauncher launcher = new()
            {
                Name = "- SCE Launcher v0.3.1 -",
                Packages = new()
                {
                    new NativePKG(),
                    new ConsolePKG(),
                    new ExternalPKG(),
                    new AliasPKG(), 
                    new VariablePKG(),
                    new ToolsPKG(),
                    new CombinePKG(),
                },
            };

            string aScripts = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "autoscripts");
            if (Directory.Exists(aScripts))
                launcher.SExecuteCommand("scrrundir", new[] { aScripts });
            else
                Directory.CreateDirectory(aScripts);

            launcher.Run();
        }
    }
}