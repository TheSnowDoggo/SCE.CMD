using CSUtils;
using System.Diagnostics;
using System.Text;
namespace SCE
{
    internal class NativePKG : Package
    {
        public NativePKG()
        {
            Name = "Native";
            Version = "1.3.0";
            Commands = new()
            {
                #region Main

                { "version", new(VersionCMD) {
                    Description = "Outputs the version of the launcher." } },

                { "quit", new(QuitCMD) {
                    Description = "Stops all processes." } },

                { "quitlauncher", new((args, cb) => cb.Launcher.Exit()) {
                    Description = "Exits the launcher." } },

                { "help", new(HelpCMD) { MaxArgs = -1,
                    Description = "Displays help info for every command in the given packages.",
                    Usage = "?<PackageName1>..." } },

                { "helpexp", new(HelpExpCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Exports help info for every command in the given packages to a file.",
                    Usage = "<FilePath> ?<PackageName1>..." } },

                { "helpexpdir", new(HelpExpDirCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Exports help info for every command in the given packages to files in a directory.",
                    Usage = "<DirectoryPath> ?<PackageName1>..." } },

                { "helpcmd", new(HelpCMDCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Displays help info for the given commands.",
                    Usage = "<CommandName>..." } },

                { "helpcmdexp", new(HelpCMDExpCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Exports help info for the given commands to a file." } },

                { "pkgview", new(PackageViewCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Outputs whether a package with the specified name exists.",
                    Usage = "<PackageName>" } },

                { "pkgversion", new(PackageVersionCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Outputs the version of the given package.",
                    Usage = "<PackageName>" } },

                { "packages", new(PackagesCMD) {
                    Description = "Displays all loaded packages." } },

                { "cmdexists", new(CommandExistsCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Determines whether the given command exists.",
                    Usage = "<CommandName>" } },

                { "proc", new(ProcCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Starts the specified process.",
                    Usage = "<FileName> ?<Arg1>..."} },

                { "!s", new(SaveCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Saves the given command until oversaved.",
                    Usage = "<CommandName> ?<Arg1>..." } },

                { "!r", new(LoadCMD) {
                    Description = "Runs the saved command." } },

                { "!c", new(ClearCMD) {
                    Description = "Clears the saved command." } },

                { "abort", new(args => throw new Exception("Aborted.")) {
                    Description = "Ends execution of a command chain." } },

                #endregion

                #region Feedback

                { "isfeed", new(GetFeedCMD) {
                    Description = "Adds the feed state to memory." } },

                { "feedback", new(FeedbackCMD(false)) { MinArgs = 1, MaxArgs = -1,
                    Description = "Feedbacks the given arguments.",
                    Usage = "<Output1>..." } },

                { "feedbackl", new(FeedbackCMD(true)) { MinArgs = 0, MaxArgs = -1,
                    Description = "Feedbacks the given arguments on new lines.",
                    Usage = "<Output1>..." } },

                { "showfeed", new(ShowFeedCMD) {MaxArgs = 1,
                    Description = "Sets whether command feedback should be displayed.",
                    Usage = "?<True/False->Toggle>" } },

                { "showerror", new(ShowErrorsCMD) { MaxArgs = 1,
                    Description = "Sets whether error feedback should be displayed.",
                    Usage = "?<True/False->Toggle>" } },

                { "neaterror", new(NeatErrorsCMD) { MaxArgs = 1,
                    Description = "Sets whether errors should only display their message.",
                    Usage = "?<True/False->Toggle>" } },

                #endregion

                #region Cache

                { "cacheclear", new(CacheClearCMD) {
                    Description = "Clears the command cache." } },

                { "cachesize", new(CacheSizeCMD) {
                    Description = "Outputs the number of items in the command cache." } },

                { "usecache", new(CacheEnabledCMD) { MaxArgs = 1,
                    Description = "Sets whether command caching should be enabled.",
                    Usage = "?<True/False->Toggle>" } },

                #endregion

                #region Chain

                { "runall", new(RunAllCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Runs every given command",
                    Usage = "<Command1>..." } },

                { "loop", new(LoopCMD) { MinArgs = 2, MaxArgs = -1,
                    Description = "Runs the command a given amount of times.",
                    Usage = "<Count> <Command> ?<Arg1>..." } },

                { "catch", new(CatchCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Catches command execution errors. Useful in command chains.",
                    Usage = "<Command> ?<Arg1>..." } },

                { "if", new(IfCMD) { MinArgs = 2, MaxArgs = -1,
                    Description = "Runs the command if the condition is true.",
                    Usage = "<True/False> <Command> ?<Arg1>..."} },

                { "async", new(AsyncCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Runs the given command on a new thread.",
                    Usage = "<CommandName> ?<Arg1>..." } },

                { "nofeed", new(NoFeedCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Runs the following command without command feedback.",
                    Usage = "<CommandName> ?<Arg1>..." } },

                #endregion

                #region Time

                { "time", new(TimeCMD) { MaxArgs = 1,
                    Description = "Gets the current time or a constant specified by the given argument.",
                    Usage = "<local>:<utc>:<today>:<unixepoch>" } },

                { "sleep", new(SleepCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Sleeps the current thread for the given amount of time in milliseconds.",
                    Usage = "<(int)Time(ms)>" } },

                { "waitms", new(WaitMSCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Precisely waits for a given amount of time in milliseconds.",
                    Usage = "<(double)Time(ms)>" } },

                { "waits", new(WaitSCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Precisely waits for a given amount of time in seconds.",
                    Usage = "<(double)Time(s)>" } },

                #endregion
            };
        }

        private static string BuildPackageHelp(Package pkg)
        {
            StringBuilder sb = new();
            sb.AppendLine(pkg.Name == "" ? "Anonymous Package:\n" : $"{pkg.Name}:\n");
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

            if (c.MinArgs != c.MaxArgs)
                sb.AppendLine($"{name}[{c.MinArgs}-{(c.MaxArgs >= 0 ? c.MaxArgs : "n")}]");
            else
                sb.AppendLine($"{name}[{c.MinArgs}]");

            if (c.Description != string.Empty)
                sb.AppendLine($"- {c.Description}");
            if (c.Usage != string.Empty)
                sb.AppendLine($"> {name} {c.Usage}");

            return sb.ToString();
        }

        private static IEnumerable<Package> ReadPackages(string[] args, Cmd.Callback cb)
        {
            if (args.Length == 0)
                return cb.Launcher.Packages();
            List<Package> packages = new(args.Length);
            foreach (var name in args)
            {
                if (!cb.Launcher.TryGetPackage(name, out var pkg))
                    throw new CmdException("Launcher", $"Unknown package \'{name}\'.");
                packages.Add(pkg);
            }
            return packages;
        }

        private static string BuildHelp(string[] args, Cmd.Callback cb)
        {
            StringBuilder sb = new("- Commands -\n");
            foreach (var pkg in ReadPackages(args, cb))
                sb.Append(BuildPackageHelp(pkg));
            return sb.ToString();
        }

        private static string BuildHelpCMD(string[] args, Cmd.Callback cb)
        {
            StringBuilder sb = new();
            bool first = true;
            foreach (var name in args)
            {
                if (!cb.Launcher.TryGetCommand(name, out var cmd, out var pkg))
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

        #region MainCommands

        private static Cmd.MemItem VersionCMD(string[] args, Cmd.Callback cb)
        {
            cb.Launcher.FeedbackLine($"Version: {CmdLauncher.VERSION}");
            return new(CmdLauncher.VERSION);
        }

        private static void QuitCMD(string[] args)
        {
            int code = args.Length > 0 ? int.Parse(args[0]) : 0;
            Environment.Exit(code);
        }

        private static void HelpCMD(string[] args, Cmd.Callback cb)
        {
            Console.Write(BuildHelp(args, cb));
        }

        private static void HelpExpCMD(string[] args, Cmd.Callback cb)
        {
            var help = BuildHelp(Utils.TrimFirst(args), cb);
            File.WriteAllText(args[0], help);
            cb.Launcher.FeedbackLine($"Successfully exported commands to:\n{args[0]}");
        }

        private static void HelpExpDirCMD(string[] args, Cmd.Callback cb)
        {
            var packages = ReadPackages(Utils.TrimFirst(args), cb).ToArray();
            if (packages.Length == 0)
                throw new CmdException("Native", "No packages selected.");
            Directory.CreateDirectory(args[0]);
            foreach (var pkg in packages)
            {
                var path = Path.Combine(args[0], $"{pkg.Name}.txt");
                File.WriteAllText(path, BuildPackageHelp(pkg));
            }
            cb.Launcher.FeedbackLine($"Successfully created directory with {packages.Length} file(s) at:\n{args[0]}");
        }

        private static void HelpCMDCMD(string[] args, Cmd.Callback cb)
        {
            Console.Write(BuildHelpCMD(args, cb));
        }

        private static void HelpCMDExpCMD(string[] args, Cmd.Callback cb)
        {
            var help = BuildHelp(Utils.TrimFirst(args), cb);
            File.WriteAllText(args[0], help);
            cb.Launcher.FeedbackLine($"Successfully exported commands to:\n{args[0]}");
        }

        private Cmd.MemItem PackageViewCMD(string[] args, Cmd.Callback cb)
        {
            if (cb.Launcher.TryGetPackage(args[0], out var package))
            {
                cb.Launcher.FeedbackLine($"Found package \'{package.Name}\' with {package.Commands.Count} command(s).");
                return new(true);
            }
            else
            {
                cb.Launcher.FeedbackLine($"No package with name \'{args}\' found.");
                return new(false);
            }
        }

        private Cmd.MemItem PackageVersionCMD(string[] args, Cmd.Callback cb)
        {
            if (!cb.Launcher.TryGetPackage(args[0], out var pkg))
                throw new CmdException("Native", $"Unknown package \'{args[0]}\'.");
            cb.Launcher.FeedbackLine($"Version: {pkg.Version}");
            return new(pkg.Version);
        }

        private void PackagesCMD(string[] args, Cmd.Callback cb)
        {
            int longest = StrUtils.Longest(cb.Launcher.Packages(), p => p.Name.Length);
            StringBuilder sb = new();
            int total = 0;
            foreach (var pkg in cb.Launcher.Packages())
            {
                int count = pkg.Commands.Count;
                var name = pkg.Name == "" ? "Unnamed*" : pkg.Name;
                sb.AppendLine($"{Utils.FTL(name, longest)} | Commands: {count}");
                total += count;
            }
            sb.AppendLine($"Total commands: {total}");
            Console.Write(sb.ToString());
        }

        private Cmd.MemItem CommandExistsCMD(string[] args, Cmd.Callback cb)
        {
            var exists = cb.Launcher.CommandExists(args[0]);
            cb.Launcher.FeedbackLine(exists);
            return new(exists);
        }

        private static void ProcCMD(string[] args)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = args[0],
                Arguments = Utils.Build(Utils.TrimFirst(args)),
                UseShellExecute = true
            });
        }

        private string[] _save = Array.Empty<string>();

        private void SaveCMD(string[] args, Cmd.Callback cb)
        {
            cb.Launcher.ExecuteCommand(args[0], Utils.TrimFirst(args));
            _save = args;
        }

        private void LoadCMD(string[] args, Cmd.Callback cb)
        {
            if (_save.Length == 0)
                throw new CmdException("Native", "No command saved.");
            cb.Launcher.ExecuteCommand(_save[0], Utils.TrimFirst(_save));
        }

        private void ClearCMD(string[] args, Cmd.Callback cb)
        {
            if (_save.Length == 0)
                throw new CmdException("Native", "No command to clear.");
            _save = Array.Empty<string>();
        }

        #endregion

        #region Feedback

        private Cmd.MemItem GetFeedCMD(string[] args, Cmd.Callback cb)
        {
            return new(cb.Launcher.CommandFeedback);
        }

        private static Action<string[], Cmd.Callback> FeedbackCMD(bool newLine)
        {
            return (args, cb) =>
            {
                if (!cb.Launcher.CommandFeedback)
                    return;
                StringBuilder sb = new();
                foreach (var arg in args)
                    sb.Append(newLine ? $"{arg}\n" : arg);
                Console.Write(sb.ToString());
            };
        }

        private static void ShowFeedCMD(string[] args, Cmd.Callback cb)
        {
            bool set = !cb.Launcher.CommandFeedback;
            if (args.Length > 0 && !bool.TryParse(args[0], out set))
                throw new CmdException("Native", $"Cannot convert \'{args[0]}\' to bool.");
            cb.Launcher.CommandFeedback = set;
        }

        private static void ShowErrorsCMD(string[] args, Cmd.Callback cb)
        {
            bool set = !cb.Launcher.ErrorFeedback;
            if (args.Length > 0 && !bool.TryParse(args[0], out set))
                throw new CmdException("Native", $"Cannot convert \'{args[0]}\' to bool.");
            cb.Launcher.ErrorFeedback = set;
            cb.Launcher.FeedbackLine($"Error feedback set to {set}.");
        }

        private static void NeatErrorsCMD(string[] args, Cmd.Callback cb)
        {
            bool set = !cb.Launcher.NeatExport;
            if (args.Length > 0 && !bool.TryParse(args[0], out set))
                throw new CmdException("Native", $"Cannot convert \'{args[0]}\' to bool.");
            cb.Launcher.NeatExport = set;
            cb.Launcher.FeedbackLine($"Neat export set to {set}.");
        }

        #endregion

        #region CacheCommands

        private static void CacheClearCMD(string[] args, Cmd.Callback cb)
        {
            int count = cb.Launcher.ClearCache();
            if (count == 0)
                throw new CmdException("Native", "No items to clear.");
            cb.Launcher.FeedbackLine($"Successfully cleared {count} item(s) from the command cache.");
        }

        private static Cmd.MemItem CacheSizeCMD(string[] args, Cmd.Callback cb)
        {
            int count = cb.Launcher.CacheSize();
            if (count == 0)
                cb.Launcher.FeedbackLine("Command cache is empty.");
            else
                cb.Launcher.FeedbackLine($"Command Cache contains {count} item(s).");
            return new(count);
        }

        private static void CacheEnabledCMD(string[] args, Cmd.Callback cb)
        {
            bool set = !cb.Launcher.CommandCaching;
            if (args.Length > 0 && !bool.TryParse(args[0], out set))
                throw new CmdException("Native", $"Cannot convert \'{args[0]}\' to bool.");
            cb.Launcher.CommandCaching = set;
            cb.Launcher.FeedbackLine($"Command Caching set to {set}.");
        }

        #endregion

        #region ChainCommands

        private static void RunAllCMD(string[] args, Cmd.Callback cb)
        {
            cb.Launcher.ExecuteEveryCommand(args);
        }

        private static void LoopCMD(string[] args, Cmd.Callback cb)
        {
            if (!int.TryParse(args[0], out var loops))
                throw new CmdException("Launcher", $"Invalid loops \'{args[0]}\'.");
            for (int i = 0; i < loops; ++i)
                if (!cb.Launcher.SExecuteCommand(args[1], Utils.TrimFromStart(args, 2)))
                    throw new CmdException("Launcher", "Loop ended as command failed to execute.");
        }

        private static void CatchCMD(string[] args, Cmd.Callback cb)
        {
            try
            {
                cb.Launcher.ExecuteCommand(args[0], Utils.TrimFirst(args));
            }
            catch
            {}
        }

        private static void IfCMD(string[] args, Cmd.Callback cb)
        {
            if (!bool.TryParse(args[0], out var result))
                throw new CmdException("Launcher", $"Cannot convert \'{args[0]}\' to bool.");
            if (result)
                cb.Launcher.ExecuteCommand(args[1], Utils.TrimFromStart(args, 2));
        }

        private static void AsyncCMD(string[] args, Cmd.Callback cb)
        {
            Thread thread = new(() =>
            {
                cb.Launcher.SExecuteCommand(args[0], Utils.TrimFirst(args));
            });
            thread.Start();
        }

        private static void NoFeedCMD(string[] args, Cmd.Callback cb)
        {
            bool prev = cb.Launcher.CommandFeedback;
            cb.Launcher.CommandFeedback = false;
            cb.Launcher.ExecuteCommand(args[0], Utils.TrimFirst(args));
            cb.Launcher.CommandFeedback = prev;
        }

        #endregion

        #region TimeCommands

        private static Cmd.MemItem TimeCMD(string[] args, Cmd.Callback cb)
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
            cb.Launcher.FeedbackLine(time);
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
