using System.Diagnostics;
using System.Text;
namespace SCE
{
    internal class NativePKG : Package
    {
        public NativePKG()
        {
            Name = "Native";
            Commands = new()
            {
                { "help", new(HelpCMD) { MaxArgs = -1,
                    Description = "Displays every command.",
                    Usage = "?<PackageName>..." } },

                { "helpcmd", new(HelpCMDCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Displays help info for the given commands.",
                    Usage = "<CommandName>..." } },

                { "haspkg", Cmd.QCommand<string>(HasPackageCMD,
                    "Displays whether a package with the specified name exists.") },

                { "packages", new(PackagesCMD) {
                    Description = "Displays all loaded packages." } },

                { "quit", new((args, cb) => cb.Launcher.Exit()) { 
                    Description = "Exits the command line." } },

                { "proc", new(args => Process.Start(new ProcessStartInfo() { FileName = args[0], 
                    Arguments = StrUtils.Build(ArrUtils.TrimFirst(args)), UseShellExecute = true })) {
                    MinArgs = 1, MaxArgs = -1, 
                    Description = "Starts the specified process.",
                    Usage = "<FileName> ?<Arg1>..."} },

                { "isfeed", new(GetFeedCMD) {
                    Description = "Adds the feed state to memory." } },

                { "feedback", new(FeedbackCMD(false)) { MinArgs = 1, MaxArgs = -1,
                    Description = "Feedbacks the given arguments.",
                    Usage = "<Output1>..." } },

                { "feedbackl", new(FeedbackCMD(true)) { MinArgs = 0, MaxArgs = -1,
                    Description = "Feedbacks the given arguments on new lines.",
                    Usage = "<Output1>..." } },

                { "showfeed", Cmd.QCommand<bool>((c, cb) => cb.Launcher.CommandFeedback = c,
                    "Sets whether command feedback should be displayed.") },

                { "showerror", new(ErrorsCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Sets whether error feedback should be displayed.",
                    Usage = "<True/False>" } },

                { "cmdexists", new(CommandExistsCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Determines whether the given command exists.",
                    Usage = "<CommandName>" } },

                { "loop", new(LoopCMD) { MinArgs = 2, MaxArgs = -1,
                    Description = "Runs the command a given amount of times.",
                    Usage = "<Count> <Command> ?<Arg1>..." } },

                { "catch", new(CatchCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Catches command execution errors. Useful in command chains.",
                    Usage = "<Command> ?<Arg1>..." } },

                { "abort", new(args => throw new Exception("Aborted.")) {
                    Description = "Ends execution of a command chain." } },

                { "if", new(IfCMD) { MinArgs = 2, MaxArgs = -1,
                    Description = "Runs the command if the condition is true.",
                    Usage = "<True/False> <Command> ?<Arg1>..."} },

                { "runall", new((args, cb) => cb.Launcher.ExecuteEveryCommand(args)) { MinArgs = 1, MaxArgs = -1,
                    Description = "Runs every given command",
                    Usage = "<Command1>..." } },

                { "async", new(AsyncCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Runs the given command on a new thread.",
                    Usage = "<CommandName> ?<Arg1>..." } },

                { "time", new(TimeCMD) { MaxArgs = 1,
                    Description = "Gets the current time or a constant specified by the given argument.",
                    Usage = "<local>:<utc>:<today>:<unixepoch>" } },              
            };
        }

        private static string BuildCMD(string name, Cmd c)
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

        private static string BuildPackageHelp(Package pkg)
        {
            StringBuilder sb = new();
            sb.AppendLine(pkg.Name == "" ? "Anonymous Package:\n" : $"{pkg.Name}:\n");
            foreach (var item in pkg.Commands)
                sb.AppendLine(BuildCMD(item.Key, item.Value));
            return sb.ToString();
        }

        private void HelpCMD(string[] args, Cmd.Callback cb)
        {
            StringBuilder sb = new("- Commands -\n");
            if (args.Length == 0)
            {
                foreach (var pkg in cb.Launcher.Packages)
                    sb.Append(BuildPackageHelp(pkg));
            }
            else
            {
                foreach (var name in args)
                {
                    if (!cb.Launcher.TryGetPackage(name, out var pkg))
                        throw new CmdException("Launcher", $"Unknown package \'{name}\'.");
                    sb.Append(BuildPackageHelp(pkg));
                }
            }
            Console.Write(sb.ToString());
        }

        private void HelpCMDCMD(string[] args, Cmd.Callback cb)
        {
            StringBuilder sb = new();
            foreach (var name in args)
            {
                if (!cb.Launcher.TryGetCommand(name, out var cmd, out var pkg))
                    Console.Write($"Unknown Command \'{name}\'.");
                else
                {
                    sb.Append($"{pkg.Name} | ");
                    sb.AppendLine(BuildCMD(name, cmd));
                }
            }
            Console.Write(sb.ToString());
        }

        private void HasPackageCMD(string name, Cmd.Callback cb)
        {
            if (cb.Launcher.TryGetPackage(name, out var package))
                cb.Launcher.FeedbackLine($"Found package \'{package.Name}\' with {package.Commands.Count} command(s).");
            else
                cb.Launcher.FeedbackLine($"No package with name \'{name}\' found.");
        }

        private void PackagesCMD(string[] args, Cmd.Callback cb)
        {
            StringBuilder sb = new();
            foreach (var package in cb.Launcher.Packages)
            {
                string name = package.Name == "" ? "Anonymous" : package.Name;
                sb.AppendLine($"{name}:\n  > Commands: {package.Commands.Count}");
            }
            Console.Write(sb.ToString());
        }

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

        private void ErrorsCMD(string[] args, Cmd.Callback cb)
        {
            if (!bool.TryParse(args[0], out var result))
                throw new CmdException("Native", $"Invalid boolean \'{args[0]}\'.");
            cb.Launcher.ErrorFeedback = result;
            cb.Launcher.FeedbackLine($"Error feedback set to {result}.");
        }

        private Cmd.MemItem CommandExistsCMD(string[] args, Cmd.Callback cb)
        {
            var exists = cb.Launcher.CommandExists(args[0]);
            cb.Launcher.FeedbackLine(exists);
            return new(exists);
        }

        private void LoopCMD(string[] args, Cmd.Callback cb)
        {
            if (!int.TryParse(args[0], out var loops))
                throw new CmdException("Launcher", $"Invalid loops \'{args[0]}\'.");
            var cmdArgs = ArrUtils.TrimFromStart(args, 2);
            for (int i = 0; i < loops; ++i)
            {
                if (!cb.Launcher.SExecuteCommand(args[1], cmdArgs))
                    throw new CmdException("Launcher", "Loop ended as command failed to execute.");
            }
        }

        private void CatchCMD(string[] args, Cmd.Callback cb)
        {
            try
            {
                cb.Launcher.ExecuteCommand(args[0], ArrUtils.TrimFirst(args));
            }
            catch
            {
            }
        }

        private void IfCMD(string[] args, Cmd.Callback cb)
        {
            if (!bool.TryParse(args[0], out var result))
                throw new CmdException("Launcher", $"Invalid boolean \'{args[0]}\'.");
            if (result)
                cb.Launcher.SExecuteCommand(args[1], ArrUtils.TrimFromStart(args, 2));
        }

        private static void AsyncCMD(string[] args, Cmd.Callback cb)
        {
            Thread thread = new(() =>
            {
                var newArgs = ArrUtils.TrimFirst(args);
                cb.Launcher.ExecuteCommand(args[0], newArgs);
            });
            thread.Start();
        }

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
    }
}
