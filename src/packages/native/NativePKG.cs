using CSUtils;
using System.Diagnostics;
using System.Text;
namespace SCE
{
    public class NativePKG : Package
    {
        public NativePKG()
        {
            Name = "Native";
            Version = new(2, 8, 0);
            Desc = "Core inbuilt commands.";
            Commands = new()
            {
                #region Main

                { "version", new(VersionCMD) {
                    Desc = "Outputs the version of the launcher." } },

                { "quit", new(QuitCMD) {
                    Desc = "Stops all processes." } },

                { "quitlauncher", new((args, cl) => cl.Exit()) {
                    Desc = "Exits the launcher." } },

                { "reload", new(ReloadCMD) { Max = 1,
                    Desc = "Reloads the program.",
                    Usage = "?<Clear:True/False->True>" } },

                { "restart", new(RestartCMD) {
                    Desc = "Restarts the program in a new window." } },

                { "help", new(HelpCMD) { Max = -1,
                    Desc = "Displays help info for every command in the given packages.",
                    Usage = "?<PackageName1>..." } },

                { "helpexp", new(HelpExpCMD) { Min = 1, Max = -1,
                    Desc = "Exports help info for every command in the given packages to a file.",
                    Usage = "<FilePath> ?<PackageName1>..." } },

                { "helpexpdir", new(HelpExpDirCMD) { Min = 1, Max = -1,
                    Desc = "Exports help info for every command in the given packages to files in a directory.",
                    Usage = "<DirectoryPath> ?<PackageName1>..." } },

                { "helpcmd", new(HelpCMDCMD) { Min = 1, Max = -1,
                    Desc = "Displays help info for the given commands.",
                    Usage = "<CommandName>..." } },

                { "helpcmdexp", new(HelpCMDExpCMD) { Min = 1, Max = -1,
                    Desc = "Exports help info for the given commands to a file." } },

                 { "precache", new(PrecacheCMD) { Max = -1,
                    Desc = "Precaches the given packages or all if no arguments are given.",
                    Usage = "?<Package1>..." } },

                { "pkgview", new(PackageViewCMD) { Min = 1, Max = 1,
                    Desc = "Outputs whether a package with the specified name exists.",
                    Usage = "<PackageName>" } },

                { "pkgversion", new(PackageVersionCMD) { Min = 1, Max = 1,
                    Desc = "Outputs the version of the given package.",
                    Usage = "<PackageName>" } },

                { "packages", new(PackagesCMD) {
                    Desc = "Displays all loaded packages." } },

                { "cmdexists", new(CommandExistsCMD) { Min = 1, Max = 1,
                    Desc = "Determines whether the given command exists.",
                    Usage = "<CommandName>" } },

                { "procs", new(ProcGEN(true)) { Min = 1, Max = -1,
                    Desc = "Starts the specified process using shell execute.",
                    Usage = "<FileName> ?<Arg1>..."} },

                { "proc", new(ProcGEN(false)) { Min = 1, Max = -1,
                    Desc = "Starts the specified process.",
                    Usage = "<FileName> ?<Arg1>..."} },

                { "!s", new(SaveCMD) { Min = 1, Max = -1,
                    Desc = "Saves the given command until oversaved.",
                    Usage = Cmd.BCHAIN } },

                { "!r", new(LoadCMD) {
                    Desc = "Runs the saved command." } },

                { "!c", new(ClearCMD) {
                    Desc = "Clears the saved command." } },

                { "abort", new(AbortCMD) { Max = -1,
                    Desc = "Ends execution of a command chain.",
                    Usage = "?<MsgPart1>..." } },

                { "mod", new(ModCMD) { Max = -1,
                    Desc = "Performs a mod operation from the last 2 memory items (top item is last arg).",
                    Usage = Cmd.MBCHAIN } },

                { "mod*", new(ModArgCMD) { Min = 2, Max = 2,
                    Desc = "Performs a mod operation on the given args.",
                    Usage = "<a> <b>" } },

                { "ignore", new(_ => { }) { Max = -1,
                    Desc = "Does literally nothing." } },

                #endregion

                #region Feedback

                { "feedback", new(FeedbackCMD(false)) { Min = 1, Max = -1,
                    Desc = "Feedbacks the given arguments.",
                    Usage = "<Output1>..." } },

                { "feedbackl", new(FeedbackCMD(true)) { Min = 0, Max = -1,
                    Desc = "Feedbacks the given arguments on new lines.",
                    Usage = "<Output1>..." } },

                #endregion

                #region Cache

                { "cacheclear", new(CacheClearCMD) {
                    Desc = "Clears the command cache." } },

                { "cachesize", new(CacheSizeCMD) {
                    Desc = "Outputs the number of items in the command cache." } },

                #endregion

                #region Chain

                { "runall", new(RunAllCMD) { Min = 1, Max = -1,
                    Desc = "Runs every given command",
                    Usage = "<Command1>..." } },

                { "loop", new(LoopCMD) { Min = 2, Max = -1,
                    Desc = "Runs the command a given amount of times.",
                    Usage = "<Count> " + Cmd.BCHAIN } },

                { "async", new(AsyncCMD) { Min = 1, Max = -1,
                    Desc = "Runs the given command on a new thread.",
                    Usage = Cmd.BCHAIN } },

                { "nofeed", new(NoFeedCMD) { Min = 1, Max = -1,
                    Desc = "Runs the following command without command feedback.",
                    Usage = Cmd.BCHAIN } },

                { "noexcept", new(NoExceptCMD) { Min = 1, Max = -1,
                    Desc = "Catches command execution errors without error feedback.",
                    Usage = Cmd.BCHAIN } },

                 { "noexcept?", new(NoExceptOCMD) { Min = 1, Max = -1,
                    Desc = "Catches command execution errors without error feedback and outputs whether an error was caught.",
                    Usage = Cmd.BCHAIN } },

                { "catch", new(CatchCMD) { Min = 1, Max = -1,
                    Desc = "Catches command execution errors.",
                    Usage = Cmd.BCHAIN } },

                { "catch?", new(CatchOCMD) { Min = 1, Max = -1,
                    Desc = "Catches command execution errors and outputs whether an error was caught.",
                    Usage = Cmd.BCHAIN } },

                { "jview", new(JViewGEN(false)) { Min = 1, Max = -1,
                    Desc = "Peeks and prints the last item in memory after running the given command.",
                    Usage = Cmd.BCHAIN } },

                { "jview^", new(JViewGEN(true)) { Min = 1, Max = -1,
                    Desc = "Pops and prints the last item in memory after running the given command.",
                    Usage = Cmd.BCHAIN } },

                #endregion

                #region Convert

                { "convt*", new(ConvArgCMD) { Min = 2, Max = 2,
                    Desc = "Converts a given argument to the given type.",
                    Usage = "<Target> <Type>" } },

                { "convt", new(ConvCMD) { Min = 1, Max = 1,
                    Desc = "Converts the last mem item to the given type.",
                    Usage = "<Type>" } },

                #endregion 

                #region Time

                { "time", new(TimeCMD) { Max = 1,
                    Desc = "Gets the current time or a constant specified by the given argument.",
                    Usage = "<local>:<utc>:<today>:<unixepoch>" } },

                { "sleep", new(SleepCMD) { Min = 1, Max = 1,
                    Desc = "Sleeps the current thread for the given amount of time in milliseconds.",
                    Usage = "<(int)Time(ms)>" } },

                { "waitms", new(WaitMSCMD) { Min = 1, Max = 1,
                    Desc = "Precisely waits for a given amount of time in milliseconds.",
                    Usage = "<(double)Time(ms)>" } },

                { "waits", new(WaitSCMD) { Min = 1, Max = 1,
                    Desc = "Precisely waits for a given amount of time in seconds.",
                    Usage = "<(double)Time(s)>" } },

                #endregion
            };
        }

        private static string BuildPackageHelp(Package pkg)
        {
            StringBuilder sb = new();
            sb.AppendLine($"- {pkg.Name} | Version: {pkg.Version} -\n{pkg.Desc}\n");
            bool first = true;
            foreach (var item in pkg.Commands)
            {
                if (!first)
                    sb.AppendLine();
                else
                    first = false;
                sb.Append(BuildCommand(item.Key, item.Value));
            }
            return sb.ToString();
        }

        private static string BuildCommand(string name, Cmd c)
        {
            StringBuilder sb = new();

            if (c.Min != c.Max)
                sb.AppendLine($"{name}[{c.Min}-{(c.Max >= 0 ? c.Max : "n")}]");
            else
                sb.AppendLine($"{name}[{c.Min}]");

            if (c.Desc != "")
                sb.AppendLine($"- {c.Desc}");
            if (c.Usage != "")
                sb.AppendLine($"> {name} {c.Usage}");
            if (c.Version != "")
                sb.AppendLine($"# Version: {c.Version}");

            return sb.ToString();
        }

        private static IEnumerable<Package> ReadPackages(string[] args, CmdLauncher cl)
        {
            if (args.Length == 0)
                return cl.Packages();
            List<Package> packages = new(args.Length);
            foreach (var name in args)
            {
                if (!cl.TryGetPackage(name, out var pkg))
                    throw new CmdException("Launcher", $"Unknown package \'{name}\'.");
                packages.Add(pkg);
            }
            return packages;
        }

        private static string BuildHelp(string[] args, CmdLauncher cl)
        {
            StringBuilder sb = new("- Commands -\n");
            foreach (var pkg in ReadPackages(args, cl))
                sb.Append(BuildPackageHelp(pkg));
            return sb.ToString();
        }

        #region MainCommands

        private static Cmd.MItem ModCMD(string[] args, CmdLauncher cl)
        {
            var o1 = MemObj(cl, true);
            if (o1 is not int b)
                b = Convert.ToInt32(o1);
            var o2 = MemObj(cl, true);
            if (o2 is not int a)
                a = Convert.ToInt32(o2);
            return new(Utils.Mod(a, b));
        }

        private static Cmd.MItem ModArgCMD(string[] args, CmdLauncher cl)
        {
            return new(Utils.Mod(int.Parse(args[0]), int.Parse(args[1])));
        }

        private static string BuildHelpCMD(string[] args, CmdLauncher cl)
        {
            StringBuilder sb = new();
            bool first = true;
            foreach (var name in args)
            {
                if (!cl.TryGetCommand(name, out var cmd, out var pkg))
                {
                    Console.Write($"Unknown Command \'{name}\'.");
                    continue;
                }
                if (!first)
                    sb.AppendLine();
                else
                    first = false;
                sb.Append($"{pkg.Name} | {BuildCommand(name, cmd)}");
            }
            return sb.ToString();
        }


        private static Cmd.MItem VersionCMD(string[] args, CmdLauncher cl)
        {
            cl.FeedbackLine($"Version: {cl.Version}");
            return new(cl.Version);
        }

        private static void QuitCMD(string[] args)
        {
            int code = args.Length > 0 ? int.Parse(args[0]) : 0;
            Environment.Exit(code);
        }

        private static void ReloadCMD(string[] args)
        {
            var clear = true;
            if (args.Length > 0 && !bool.TryParse(args[0], out clear))
                throw new CmdException("Native", $"Invalid bool \'{args[0]}\'.");
            if (clear)
                Console.Clear();
            Process.Start(Environment.ProcessPath ?? 
                throw new CmdException("Native", "Could not resolve executable path."));
            Environment.Exit(0);
        }

        private static void RestartCMD(string[] args)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = Environment.ProcessPath ??
                        throw new CmdException("Native", "Could not resolve executable path."),
                UseShellExecute = true,
            });
            Environment.Exit(0);
        }

        private static void HelpCMD(string[] args, CmdLauncher cl)
        {
            Console.Write(BuildHelp(args, cl));
        }

        private static void HelpExpCMD(string[] args, CmdLauncher cl)
        {
            var help = BuildHelp(Utils.TrimFirst(args), cl);
            File.WriteAllText(args[0], help);
            cl.FeedbackLine($"Successfully exported commands to:\n{args[0]}");
        }

        private static void HelpExpDirCMD(string[] args, CmdLauncher cl)
        {
            var packages = ReadPackages(Utils.TrimFirst(args), cl).ToArray();
            if (packages.Length == 0)
                throw new CmdException("Native", "No packages selected.");
            Directory.CreateDirectory(args[0]);
            foreach (var pkg in packages)
            {
                var path = Path.Combine(args[0], $"{pkg.Name}.txt");
                File.WriteAllText(path, BuildPackageHelp(pkg));
            }
            cl.FeedbackLine($"Successfully created directory with {packages.Length} file(s) at:\n{args[0]}");
        }

        private static void HelpCMDCMD(string[] args, CmdLauncher cl)
        {
            Console.Write(BuildHelpCMD(args, cl));
        }

        private static void HelpCMDExpCMD(string[] args, CmdLauncher cl)
        {
            var help = BuildHelp(Utils.TrimFirst(args), cl);
            File.WriteAllText(args[0], help);
            cl.FeedbackLine($"Successfully exported commands to:\n{args[0]}");
        }

        private static void PrecacheCMD(string[] args, CmdLauncher cl)
        {
            int count = 0;
            foreach (var pkg in ReadPackages(args, cl))
            {
                cl.Precache(pkg);
                ++count;
            }
            cl.FeedbackLine($"Successfully precached {count} package(s) | Cache size: {cl.CacheSize}");
        }

        private Cmd.MItem PackageViewCMD(string[] args, CmdLauncher cl)
        {
            if (cl.TryGetPackage(args[0], out var package))
            {
                cl.FeedbackLine($"Found package \'{package.Name}\' with {package.Commands.Count} command(s).");
                return new(true);
            }
            else
            {
                cl.FeedbackLine($"No package with name \'{args}\' found.");
                return new(false);
            }
        }

        private Cmd.MItem PackageVersionCMD(string[] args, CmdLauncher cl)
        {
            if (!cl.TryGetPackage(args[0], out var pkg))
                throw new CmdException("Native", $"Unknown package \'{args[0]}\'.");
            cl.FeedbackLine($"Version: {pkg.Version}");
            return new(pkg.Version);
        }

        private void PackagesCMD(string[] args, CmdLauncher cl)
        {
            int longest = StrUtils.Longest(cl.Packages(), p => p.Name.Length);
            StringBuilder sb = new();
            int total = 0;
            foreach (var pkg in cl.Packages())
            {
                int count = pkg.Commands.Count;
                var name = pkg.Name == "" ? "Unnamed*" : pkg.Name;
                sb.AppendLine($"{Utils.FTL(name, longest)} | Commands: {count}");
                total += count;
            }
            sb.AppendLine($"Total commands: {total}");
            Console.Write(sb.ToString());
        }

        private Cmd.MItem CommandExistsCMD(string[] args, CmdLauncher cl)
        {
            var exists = cl.ContainsCommand(args[0]);
            cl.FeedbackLine(exists);
            return new(exists);
        }

        private static Action<string[]> ProcGEN(bool shell)
        {
            return args =>
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = args[0],
                    Arguments = Utils.Build(Utils.TrimFirst(args)),
                    UseShellExecute = shell
                });
            };
        }

        private string[] _save = Array.Empty<string>();

        private void SaveCMD(string[] args, CmdLauncher cl)
        {
            cl.ExecuteCommand(args[0], Utils.TrimFirst(args));
            _save = args;
        }

        private void LoadCMD(string[] args, CmdLauncher cl)
        {
            if (_save.Length == 0)
                throw new CmdException("Native", "No command saved.");
            cl.ExecuteCommand(_save[0], Utils.TrimFirst(_save));
        }

        private void ClearCMD(string[] args, CmdLauncher cl)
        {
            if (_save.Length == 0)
                throw new CmdException("Native", "No command to clear.");
            _save = Array.Empty<string>();
        }

        #endregion

        #region Feedback

        private static Action<string[], CmdLauncher> FeedbackCMD(bool newLine)
        {
            return (args, cl) =>
            {
                if (!cl.CmdFeedback)
                    return;
                StringBuilder sb = new();
                foreach (var arg in args)
                    sb.Append(newLine ? $"{arg}\n" : arg);
                Console.Write(sb.ToString());
            };
        }

        #endregion

        #region CacheCommands

        private static void CacheClearCMD(string[] args, CmdLauncher cl)
        {
            int count = cl.ClearCache();
            if (count == 0)
                throw new CmdException("Native", "No items to clear.");
            cl.FeedbackLine($"Successfully cleared {count} item(s) from the command cache.");
        }

        private static Cmd.MItem CacheSizeCMD(string[] args, CmdLauncher cl)
        {
            int count = cl.CacheSize;
            if (count == 0)
                cl.FeedbackLine("Command cache is empty.");
            else
                cl.FeedbackLine($"Command Cache contains {count} item(s).");
            return new(count);
        }

        #endregion

        #region ChainCommands

        private static void AbortCMD(string[] args, CmdLauncher cl)
        {
            throw new CmdException("Native", args.Length > 0 ? Utils.Infill(args, " ") : "Abort called.");
        }

        private static void RunAllCMD(string[] args, CmdLauncher cl)
        {
            cl.ExecuteEveryCommand(args);
        }

        private static void LoopCMD(string[] args, CmdLauncher cl)
        {
            if (!int.TryParse(args[0], out var loops))
                throw new CmdException("Launcher", $"Invalid loops \'{args[0]}\'.");
            for (int i = 0; i < loops; ++i)
                if (!cl.SExecuteCommand(args[1], Utils.TrimFromStart(args, 2)))
                    throw new CmdException("Launcher", "Loop ended as command failed to execute.");
        }

        private static void CatchCMD(string[] args, CmdLauncher cl)
        {
            cl.SExecuteCommand(args[0], Utils.TrimFirst(args));
        }

        private static Cmd.MItem CatchOCMD(string[] args, CmdLauncher cl)
        {
            return new(!cl.SExecuteCommand(args[0], Utils.TrimFirst(args)));
        }

        private static Action<string[], CmdLauncher> JViewGEN(bool pop)
        {
            return (args, cl) =>
            {
                cl.ExecuteCommand(args[0], Utils.TrimFirst(args));
                Console.WriteLine(MemStr(cl, pop));
            };
        }

        private static void AsyncCMD(string[] args, CmdLauncher cl)
        {
            Thread thread = new(() =>
            {
                cl.SExecuteCommand(args[0], Utils.TrimFirst(args));
            });
            thread.Start();
        }

        private static void NoFeedCMD(string[] args, CmdLauncher cl)
        {
            bool prev = cl.CmdFeedback;
            cl.CmdFeedback = false;
            cl.SExecuteCommand(args[0], Utils.TrimFirst(args));
            cl.CmdFeedback = prev;
        }

        private static void NoExceptCMD(string[] args, CmdLauncher cl)
        {
            bool prev = cl.ErrFeedback;
            cl.ErrFeedback = false;
            try
            {
                cl.ExecuteCommand(args[0], Utils.TrimFirst(args));
            }
            catch
            { }
            cl.ErrFeedback = prev;
        }

        private static Cmd.MItem NoExceptOCMD(string[] args, CmdLauncher cl)
        {
            bool prev = cl.ErrFeedback;
            cl.ErrFeedback = false;
            try
            {
                cl.ExecuteCommand(args[0], Utils.TrimFirst(args));
                cl.ErrFeedback = prev;
                return new(true);
            }
            catch
            {

                cl.ErrFeedback = prev;
                return new(false);
            }
        }

        #endregion

        #region ConvertCommands

        public static object MemObj(CmdLauncher cl, bool pop = true)
        {
            if (cl.MemoryStack.Count == 0)
                throw new CmdException("Native", "Memory stack is empty.");
            var obj = (pop ? cl.MemoryStack.Pop() : cl.MemoryStack.Peek()) ??
                throw new CmdException("Native", "Memory item is null.");
            return obj;
        }

        public static string MemStr(CmdLauncher cl, bool pop = true)
        {
            return MemObj(cl, pop).ToString() ??
                throw new CmdException("Native", "Memory string conversion is null.");
        }

        private static Cmd.MItem ConvArgCMD(string[] args)
        {
            var t = StrUtils.BetterGetType(args[1]);
            var res = Convert.ChangeType(args[0], t) ??
                throw new CmdException("Native", "Conversion resulted in null.");
            return new(res);
        }

        private static Cmd.MItem ConvCMD(string[] args, CmdLauncher cl)
        {
            var t = StrUtils.BetterGetType(args[0]);
            var res = Convert.ChangeType(MemObj(cl), t) ??
                throw new CmdException("Native", "Conversion resulted in null.");
            return new(res);
        }

        #endregion

        #region TimeCommands

        private static Cmd.MItem TimeCMD(string[] args, CmdLauncher cl)
        {
            var timeOpt = args.Length > 0 ? args[0].ToLower() : "local";
            DateTime time = timeOpt switch
            {
                "local" => DateTime.Now,
                "utc" => DateTime.UtcNow,
                "today" => DateTime.Today,
                "unixepoch" => DateTime.UnixEpoch,
                _ => throw new CmdException("Native", "Unknown time argument, try:\n> local\n> utc\n> today\n> unixepoch")
            };
            cl.FeedbackLine(time);
            return new(time);
        }

        private static void WaitSCMD(string[] args)
        {
            var dur_s = double.Parse(args[0]);
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed.TotalSeconds < dur_s) ;
        }

        private static void WaitMSCMD(string[] args)
        {
            var dur_ms = double.Parse(args[0]);
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed.TotalMilliseconds < dur_ms) ;
        }

        private static void SleepCMD(string[] args)
        {
            Thread.Sleep(int.Parse(args[0]));
        }

        #endregion
    }
}