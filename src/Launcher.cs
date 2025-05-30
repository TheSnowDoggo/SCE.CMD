using CSUtils;

namespace SCE
{
    internal static class Launcher
    {
        private static void Main()
        {
            try
            {
                PVersion ver;
                try
                {
                    ver = CmdLauncher.ResolveVersion();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to resolve version.\n{e}");
                    ver = PVersion.Unknown;
                }

                CmdLauncher cl = new() { Version = ver };

                cl.SLoadPackages(CmdLauncher.NativePackages);

                var scrPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "autoscripts");
                if (Directory.Exists(scrPath))
                {
                    cl.SExecuteCommand("scrrundir", new[] { scrPath });
                }
                else
                {
                    Directory.CreateDirectory(scrPath);
                }

                Console.WriteLine($"- SCE Launcher v{cl.Version} -\n" +
                    $"Start typing or type help to see available commands:");
                cl.Run();
            }
            catch (Exception e)
            {
                StrUtils.PrettyErr("Main", "An exception was thrown during launcher initialization:");
                Console.WriteLine(e.Message);
                if (Utils.BoolPrompt("Would you like to view the full error? Yes[Y] or No[N]: "))
                {
                    Console.WriteLine(e);
                }
                Utils.ContinueAny();
            }
        }
    }
}