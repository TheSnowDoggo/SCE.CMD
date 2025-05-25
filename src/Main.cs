using CSUtils;
using System.Reflection;

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

        private static PVersion ResolveVersion()
        {
            var asmb = Assembly.GetEntryAssembly() ??
                throw new NullReferenceException("Assembly is null.");
            var v = asmb.GetName().Version ??
                throw new NullReferenceException("Version is null.");
            return new(v.Major, v.Minor, v.Build);
        }

        internal static void Main()
        {
            try
            {
                PVersion ver;
                try
                {
                    ver = ResolveVersion();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to resolve version.\n{e}");
                    ver = PVersion.Unknown;
                }

                CmdLauncher cl = new() { Version = ver };
                
                cl.SLoadPackages(_native);

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