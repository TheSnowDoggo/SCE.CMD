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
            Version = "2.6.0";
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

                { "abort", new(AbortCMD) { MaxArgs = -1,
                    Description = "Ends execution of a command chain.",
                    Usage = "?<MsgPart1>..." } },

                { "mod", new(ModCMD) { MinArgs = 2, MaxArgs = 2,
                    Description = "Performs a mod operation.",
                    Usage = "<a> <b>" } },

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

                { "async", new(AsyncCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Runs the given command on a new thread.",
                    Usage = "<CommandName> ?<Arg1>..." } },

                { "nofeed", new(NoFeedCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Runs the following command without command feedback.",
                    Usage = "<CommandName> ?<Arg1>..." } },

                { "noexcept", new(NoExceptCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Catches command execution errors without error feedback.",
                    Usage = "<CommandName> ?<Arg1>..." } },

                 { "noexcept?", new(NoExceptOCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Catches command execution errors without error feedback and outputs whether an error was caught.",
                    Usage = "<CommandName> ?<Arg1>..." } },

                { "catch", new(CatchCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Catches command execution errors.",
                    Usage = "<Command> ?<Arg1>..." } },

                { "catch?", new(CatchOCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Catches command execution errors and outputs whether an error was caught.",
                    Usage = "<Command> ?<Arg1>..." } },

                { "jview", new(JViewCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Takes and feedbacks the last item in memory after running the given command.",
                    Usage = "<Command> ?<Arg1>..." } },

                #endregion

                #region Condition

                { "ifarg", new(IfArgCMD) { MinArgs = 2, MaxArgs = -1,
                    Description = "Runs the command if the argument condition is true.",
                    Usage = "<True/False:!=0->True;False> <Command> ?<Arg1>..."} },

                { "!ifarg", new(IfArgNotCMD) { MinArgs = 2, MaxArgs = -1,
                    Description = "Runs the command if the argument condition is false.",
                    Usage = "<True/False:!=0->True;False> <Command> ?<Arg1>..."} },

                { "elifarg", new(ElseIfArgCMD) { MinArgs = 3, MaxArgs = 3,
                    Description = "Runs left cmd if argument condition true; right cmd.",
                    Usage = "<True/False:!=0->True;False> <Command1> <Command2>"} },

                { "!elifarg", new(ElseIfArgNotCMD) { MinArgs = 3, MaxArgs = 3,
                    Description = "Runs left cmd if argument condition false; right cmd.",
                    Usage = "<True/False:!=0->True;False> <Command1> <Command2>"} },

                { "if", new(IfCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Runs the command if the last item in memory is true.",
                    Usage = "<Command> ?<Arg1>..."} },

                { "elif", new(ElseIfCMD)  { MinArgs = 2, MaxArgs = 2,
                    Description = "Runs left cmd if last mem item true; right cmd.",
                    Usage = "<Command1> <Command2>"} },

                { "!if", new(IfNotCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Runs the command if the last item in memory is false.",
                    Usage = "<Command> ?<Arg1>..."} },

                { "!elif", new(ElseIfNotCMD)  { MinArgs = 2, MaxArgs = 2,
                    Description = "Runs left cmd if last mem item false; right cmd.",
                    Usage = "<Command1> <Command2>"} },

                { "eql", new(EqualsCMD) { MinArgs = 2, MaxArgs = 3,
                    Description = "Outputs whether the given arguments are equal.",
                    Usage = "<Left> <Right> ?<Type:LeftType,RightType>" } },

                { "!eql", new(EqualsNotCMD) { MinArgs = 2, MaxArgs = 3,
                    Description = "Outputs whether the given arguments are not equal.",
                    Usage = "<Left> <Right> ?<Type:LeftType,RightType>" } },

                { "!res", new(NotResCMD) { MaxArgs = -1,
                    Description = "Nots and outputs the last item in memory after running the given command.",
                    Usage = "?<CommandName> ?<Arg1>..." } },

                { "cmp=", new(EQCMD) { MinArgs = 2, MaxArgs = 3,
                    Description = "Outputs whether left = right.",
                    Usage = "<Left> <Right> ?<Type:LeftType,RightType>" } },

                { "cmp!=", new(NEQCMD) { MinArgs = 2, MaxArgs = 3,
                    Description = "Outputs whether left != right.",
                    Usage = "<Left> <Right> ?<Type:LeftType,RightType>" } },

                { "cmp<", new(LTCMD) { MinArgs = 2, MaxArgs = 3,
                    Description = "Outputs whether left < right.",
                    Usage = "<Left> <Right> ?<Type:LeftType,RightType>" } },

                { "cmp>", new(GTCMD) { MinArgs = 2, MaxArgs = 3,
                    Description = "Outputs whether left > right.",
                    Usage = "<Left> <Right> ?<Type:LeftType,RightType>" } },

                { "cmp<=", new(LTECMD) { MinArgs = 2, MaxArgs = 3,
                    Description = "Outputs whether left <= right.",
                    Usage = "<Left> <Right> ?<Type:LeftType,RightType>" } },

                { "cmp>=", new(GTECMD) { MinArgs = 2, MaxArgs = 3,
                    Description = "Outputs whether left >= right.",
                    Usage = "<Left> <Right> ?<Type:LeftType,RightType>" } },

                { "not", new(NotCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Nots the given boolean.",
                    Usage = "<Boolean>" } },

                { "istypearg", new(IsTypeArgCMD) { MinArgs = 2, MaxArgs = 2,
                    Description = "Determines if the given argument is a type.",
                    Usage = "<Check> <Type>" } },

                { "istype", new(IsTypeCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Determines if the last mem item is a type.",
                    Usage = "<Type>" } },

                { "convtarg", new(ConvArgCMD) { MinArgs = 2, MaxArgs = 2,
                    Description = "Converts a given argument to the given type.",
                    Usage = "<Target> <Type>" } },

                { "convt", new(ConvCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Converts the last mem item to the given type.",
                    Usage = "<Type>" } },

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
            sb.AppendLine($"- {pkg.Name} | Version: {pkg.Version} -\n");
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

        private static Cmd.MemItem ModCMD(string[] args, Cmd.Callback cb)
        {
            int res = Utils.Mod(int.Parse(args[0]), int.Parse(args[1]));
            cb.Launcher.FeedbackLine(res);
            return new(res);
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
            var exists = cb.Launcher.ContainsCommand(args[0]);
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
            return new(cb.Launcher.CmdFeedback);
        }

        private static Action<string[], Cmd.Callback> FeedbackCMD(bool newLine)
        {
            return (args, cb) =>
            {
                if (!cb.Launcher.CmdFeedback)
                    return;
                StringBuilder sb = new();
                foreach (var arg in args)
                    sb.Append(newLine ? $"{arg}\n" : arg);
                Console.Write(sb.ToString());
            };
        }

        private static void ShowFeedCMD(string[] args, Cmd.Callback cb)
        {
            bool set = !cb.Launcher.CmdFeedback;
            if (args.Length > 0 && !bool.TryParse(args[0], out set))
                throw new CmdException("Native", $"Cannot convert \'{args[0]}\' to bool.");
            cb.Launcher.CmdFeedback = set;
        }

        private static void ShowErrorsCMD(string[] args, Cmd.Callback cb)
        {
            bool set = !cb.Launcher.ErrFeedback;
            if (args.Length > 0 && !bool.TryParse(args[0], out set))
                throw new CmdException("Native", $"Cannot convert \'{args[0]}\' to bool.");
            cb.Launcher.ErrFeedback = set;
            cb.Launcher.FeedbackLine($"Error feedback set to {set}.");
        }

        private static void NeatErrorsCMD(string[] args, Cmd.Callback cb)
        {
            bool set = !cb.Launcher.NeatErrors;
            if (args.Length > 0 && !bool.TryParse(args[0], out set))
                throw new CmdException("Native", $"Cannot convert \'{args[0]}\' to bool.");
            cb.Launcher.NeatErrors = set;
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
            bool set = !cb.Launcher.CmdCaching;
            if (args.Length > 0 && !bool.TryParse(args[0], out set))
                throw new CmdException("Native", $"Cannot convert \'{args[0]}\' to bool.");
            cb.Launcher.CmdCaching = set;
            cb.Launcher.FeedbackLine($"Command Caching set to {set}.");
        }

        #endregion

        #region ChainCommands

        private static void AbortCMD(string[] args, Cmd.Callback cb)
        {
            throw new CmdException("Native", args.Length > 0 ? Utils.Infill(args, " ") : "Abort called.");
        }

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
            cb.Launcher.SExecuteCommand(args[0], Utils.TrimFirst(args));
        }

        private static Cmd.MemItem CatchOCMD(string[] args, Cmd.Callback cb)
        {
            return new(!cb.Launcher.SExecuteCommand(args[0], Utils.TrimFirst(args)));
        }

        private static void JViewCMD(string[] args, Cmd.Callback cb)
        {
            cb.Launcher.ExecuteCommand(args[0], Utils.TrimFirst(args));
            if (cb.Launcher.MemoryStack.Count == 0)
                throw new CmdException("Native", "Memory stack is empty.");
            var obj = cb.Launcher.MemoryStack.Pop() ??
                throw new CmdException("Native", "Memory item is null.");
            var str = obj.ToString() ??
                throw new CmdException("Native", "Memory string conversion is null.");
            cb.Launcher.FeedbackLine(str);
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
            bool prev = cb.Launcher.CmdFeedback;
            cb.Launcher.CmdFeedback = false;
            cb.Launcher.SExecuteCommand(args[0], Utils.TrimFirst(args));
            cb.Launcher.CmdFeedback = prev;
        }

        private static void NoExceptCMD(string[] args, Cmd.Callback cb)
        {
            bool prev = cb.Launcher.ErrFeedback;
            cb.Launcher.ErrFeedback = false;
            try
            {
                cb.Launcher.ExecuteCommand(args[0], Utils.TrimFirst(args));
            }
            catch
            { }
            cb.Launcher.ErrFeedback = prev;
        }

        private static Cmd.MemItem NoExceptOCMD(string[] args, Cmd.Callback cb)
        {
            bool prev = cb.Launcher.ErrFeedback;
            cb.Launcher.ErrFeedback = false;
            try
            {
                cb.Launcher.ExecuteCommand(args[0], Utils.TrimFirst(args));
                return new(true);
            }
            catch
            {
                return new(false);
            }
            cb.Launcher.ErrFeedback = prev;
        }

        #endregion

        #region ConditionCommands

        private static bool Condition(string input)
        {
            if (bool.TryParse(input, out var result))
                return result;
            if (int.TryParse(input, out var num))
                return num != 0;
            throw new CmdException("Launcher", $"Invalid conditional \'{input}\'.");
        }

        private static object MemObj(Cmd.Callback cb, bool pop = true)
        {
            if (cb.Launcher.MemoryStack.Count == 0)
                throw new CmdException("Native", "Memory stack is empty.");
            var obj = (pop ? cb.Launcher.MemoryStack.Pop() : cb.Launcher.MemoryStack.Peek()) ??
                throw new CmdException("Native", "Memory item is null.");
            return obj;
        }

        private static bool MemBool(Cmd.Callback cb)
        {
            var obj = MemObj(cb);
            if (obj is bool c)
                return c;
            var str = obj.ToString() ??
                throw new CmdException("Native", "Last memory item was not a valid boolean.");
            return !Condition(str);
        }

        private static void IfArgCMD(string[] args, Cmd.Callback cb)
        {
            if (Condition(args[0]))
                cb.Launcher.ExecuteCommand(args[1], Utils.TrimFromStart(args, 2));
        }

        private static void ElseIfArgCMD(string[] args, Cmd.Callback cb)
        {
            cb.Launcher.ExecuteCommand(Condition(args[0]) ? args[1] : args[2]);
        }

        private static void IfArgNotCMD(string[] args, Cmd.Callback cb)
        {
            if (!Condition(args[0]))
                cb.Launcher.ExecuteCommand(args[1], Utils.TrimFromStart(args, 2));
        }

        private static void ElseIfArgNotCMD(string[] args, Cmd.Callback cb)
        {
            cb.Launcher.ExecuteCommand(!Condition(args[0]) ? args[1] : args[2]);
        }

        private static void IfCMD(string[] args, Cmd.Callback cb)
        {
            if (MemBool(cb))
                cb.Launcher.ExecuteCommand(args[0], Utils.TrimFirst(args));
        }

        private static void ElseIfCMD(string[] args, Cmd.Callback cb)
        {
            cb.Launcher.ExecuteCommand(MemBool(cb) ? args[0] : args[1]);
        }

        private static void IfNotCMD(string[] args, Cmd.Callback cb)
        {
            if (!MemBool(cb))
                cb.Launcher.ExecuteCommand(args[0], Utils.TrimFirst(args));
        }

        private static void ElseIfNotCMD(string[] args, Cmd.Callback cb)
        {
            cb.Launcher.ExecuteCommand(!MemBool(cb) ? args[0] : args[1]);
        }

        private static (object, object) GetAsTypes(string[] args)
        {
            if (args.Length == 2)
                return (args[0], args[1]);
            var types = args[2].Split(',');
            object o1, o2;
            if (types.Length == 1)
            {
                var t = StrUtils.BetterGetType(types[0]);
                o1 = Convert.ChangeType(args[0], t);
                o2 = Convert.ChangeType(args[1], t);
            }
            else if (types.Length == 2)
            {
                var t1 = StrUtils.BetterGetType(types[0]);
                var t2 = StrUtils.BetterGetType(types[1]);
                o1 = Convert.ChangeType(args[0], t1);
                o2 = Convert.ChangeType(args[1], t2);
            }
            else
            {
                throw new CmdException("Native", $"Invalid number of types given \'{args[0]}\'.");
            }
            return (o1, o2);
        }

        private static bool Equals(string[] args)
        {
            (var o1, var o2) = GetAsTypes(args);
            return o1.Equals(o2);
        }

        private static int Compare(string[] args)
        {
            (var o1, var o2) = GetAsTypes(args);
            if (o1 is not IComparable c1)
                throw new CmdException("Native", "Left item is not comparable.");
            return c1.CompareTo(o2);
        }

        private static Cmd.MemItem EqualsCMD(string[] args)
        {
            return new(Equals(args));
        }

        private static Cmd.MemItem EqualsNotCMD(string[] args)
        {
            return new(!Equals(args));
        }

        private static Cmd.MemItem NotResCMD(string[] args, Cmd.Callback cb)
        {
            if (args.Length > 0)
                cb.Launcher.ExecuteCommand(args[0], Utils.TrimFirst(args));
            if (cb.Launcher.MemoryStack.Count == 0)
                throw new CmdException("Native", "Memory stack is empty.");
            var obj = cb.Launcher.MemoryStack.Pop() ??
                throw new CmdException("Native", "Memory item is null.");
            if (obj is bool c)
                return new(!c);
            var str = obj.ToString() ??
                throw new CmdException("Native", "Last memory item was not a valid boolean.");
            return new(!Condition(str));
        }

        private static Cmd.MemItem EQCMD(string[] args)
        {
            return new(Compare(args) == 0);
        }

        private static Cmd.MemItem NEQCMD(string[] args)
        {
            return new(Compare(args) != 0);
        }

        private static Cmd.MemItem LTCMD(string[] args)
        {
            return new(Compare(args) < 0);
        }

        private static Cmd.MemItem GTCMD(string[] args)
        {
            return new(Compare(args) > 0);
        }

        private static Cmd.MemItem LTECMD(string[] args)
        {
            return new(Compare(args) <= 0);
        }

        private static Cmd.MemItem GTECMD(string[] args)
        {
            return new(Compare(args) >= 0);
        }

        private static Cmd.MemItem NotCMD(string[] args, Cmd.Callback cb)
        {
            return new(!Condition(args[0]));
        }

        private static Cmd.MemItem IsTypeArgCMD(string[] args)
        {
            var t = StrUtils.BetterGetType(args[1]);
            try
            {
                var res = Convert.ChangeType(args[0], t);
                return new(res != null);
            }
            catch
            {
                return new(false);
            }
        }

        private static Cmd.MemItem IsTypeCMD(string[] args, Cmd.Callback cb)
        {
            var t = StrUtils.BetterGetType(args[0]);
            return new(MemObj(cb, false).GetType() == t);
        }

        private static Cmd.MemItem ConvArgCMD(string[] args)
        {
            var t = StrUtils.BetterGetType(args[1]);
            var res = Convert.ChangeType(args[0], t) ??
                throw new CmdException("Native", "Conversion resulted in null.");
            return new(res);
        }

        private static Cmd.MemItem ConvCMD(string[] args, Cmd.Callback cb)
        {
            var t = StrUtils.BetterGetType(args[0]);
            var res = Convert.ChangeType(MemObj(cb), t) ??
                throw new CmdException("Native", "Conversion resulted in null.");
            return new(res);
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
