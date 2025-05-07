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

                { "quit", new((args, cb) => cb.Launcher.Exit()) { 
                    Description = "Exits the command line." } },

                { "proc", new(args => Process.Start(new ProcessStartInfo() { FileName = args[0], 
                    Arguments = StrUtils.Build(ArrUtils.TrimFirst(args)), UseShellExecute = true })) {
                    MinArgs = 1, MaxArgs = -1, 
                    Description = "Starts the specified process.",
                    Usage = "<FileName> ?<Arg1>..."} },

                { "showfeed", Cmd.QCommand<bool>((c, cb) => cb.Launcher.CommandFeedback = c,
                    "Sets whether command feedback should be displayed.") },

                { "cmdexists", new(CommandExistsCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Determines whether the given command exists.",
                    Usage = "<CommandName>" } },

                { "showerror", new(ErrorsCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Sets whether error feedback should be displayed.",
                    Usage = "<True/False>" } },

                { "isfeed", new(GetFeedCMD) { 
                    Description = "Adds the feed state to memory." } },

                { "feedback", new(FeedbackCMD(false)) { MinArgs = 1, MaxArgs = -1,
                    Description = "Feedbacks the given arguments.",
                    Usage = "<Output1>..." } },

                { "feedbackl", new(FeedbackCMD(true)) { MinArgs = 0, MaxArgs = -1,
                    Description = "Feedbacks the given arguments on new lines.",
                    Usage = "<Output1>..." } },

                { "loop", new(LoopCMD) { MinArgs = 2, MaxArgs = -1,
                    Description = "Runs the command a given amount of times.",
                    Usage = "<Count> <Command> ?<Arg1>..." } },

                { "catch", new(CatchCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Catches command execution errors. Useful in command chains.",
                    Usage = "<Command> ?<Arg1>..." } },

                { "runif", new(IfCMD) { MinArgs = 2, MaxArgs = -1,
                    Description = "Runs the command if the condition is true.",
                    Usage = "<True/False> <Command> ?<Arg1>..."} },

                { "abort", new(args => throw new Exception("Aborted.")) {
                    Description = "Ends execution of a command chain." } },

                { "haspkg", Cmd.QCommand<string>(HasPackageCMD,
                    "Displays whether a package with the specified name exists.") },

                { "runall", new((args, cb) => cb.Launcher.ExecuteEveryCommand(args)) { MinArgs = 1, MaxArgs = -1,
                    Description = "Runs every given command",
                    Usage = "<Command1>..." } },

                { "packages", new(PackagesCMD) { 
                    Description = "Displays all loaded packages." } },

                { "time", new(TimeCMD) { MaxArgs = 1,
                    Description = "Gets the current time or a constant specified by the given argument.",
                    Usage = "<local>:<utc>:<today>:<unixepoch>" } },

                { "memclear", new(MemClearCMD) { 
                    Description = "Clears all items in laucher memory." } },

                { "memadd", new(MemAddCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Adds every given item to memory.",
                    Usage = "<Item1>..." } },

                { "memrem", new(MemRemCMD) { MaxArgs = 1,
                    Description = "Removes the specified number of items from memory",
                    Usage = "?<ItemCount->All>" } },

                { "memview", new(MemViewCMD) { 
                    Description = "Displays all items in memory" } },

                { "memlock", new(MemLockCMD) { MaxArgs = 1, 
                    Description = "Sets the lock state of memory.",
                    Usage = "?<True/False->Toggle>" } },

                { "memcount", new(MemSizeCMD) { 
                    Description = "Outputs the number of items in the memory stack." } },

                { "memrun", new(MemRunCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Adds items to memory before running the command with no arguments.",
                    Usage = "<CommandName> ?<Item1>..." } },

                { "memins", new(MemInsCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Inserts items from memory into the command ('&'=peek '&^'=pop).",
                    Usage = "<Command> ?<Arg1>..." } },
            };
        }

        private static string BuildPackageHelp(Package pkg)
        {
            StringBuilder sb = new();
            sb.AppendLine(pkg.Name == "" ? "Anonymous Package:\n" : $"{pkg.Name}:\n");
            foreach (var item in pkg.Commands)
            {
                (var name, var c) = item;

                if (c.MinArgs != c.MaxArgs)
                    sb.AppendLine($"{name}[{c.MinArgs}-{(c.MaxArgs >= 0 ? c.MaxArgs : "n")}]");
                else
                    sb.AppendLine($"{name}[{c.MinArgs}]");

                if (c.Description != string.Empty)
                    sb.AppendLine($"- {c.Description}");
                if (c.Usage != string.Empty)
                    sb.AppendLine($"> {name} {c.Usage}");

                sb.AppendLine();
            }
            return sb.ToString();
        }

        #region Commands

        private Cmd.MemItem CommandExistsCMD(string[] args, Cmd.Callback cb)
        {
            var exists = cb.Launcher.CommandExists(args[0]);
            cb.Launcher.FeedbackLine(exists);
            return new(exists);
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

        private void ErrorsCMD(string[] args, Cmd.Callback cb)
        {
            if (!bool.TryParse(args[0], out var result))
                throw new CmdException("Native", $"Invalid boolean \'{args[0]}\'.");
            cb.Launcher.ErrorFeedback = result;
            cb.Launcher.FeedbackLine($"Error feedback set to {result}.");
        }

        private void IfCMD(string[] args, Cmd.Callback cb)
        {
            if (!bool.TryParse(args[0], out var result))
                throw new CmdException("Launcher", $"Invalid boolean \'{args[0]}\'.");
            if (result)
                cb.Launcher.SExecuteCommand(args[1], ArrUtils.TrimFromStart(args, 2));
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

        #region Memory

        private void HasPackageCMD(string name, Cmd.Callback cb)
        {
            if (cb.Launcher.TryGetPackage(name, out var package))
                cb.Launcher.FeedbackLine($"Found package \'{package.Name}\' with {package.Commands.Count} command(s).");
            else
                cb.Launcher.FeedbackLine($"No package with name \'{name}\' found.");
        }

        private void MemAddCMD(string[] args, Cmd.Callback cb)
        {
            foreach (var item in args)
                cb.Launcher.MemoryStack.Push(item);
            cb.Launcher.FeedbackLine($"Sucessfully added {args.Length} items to memory.");
        }

        private void MemRemCMD(string[] args, Cmd.Callback cb)
        {
            int count = 1;
            if (args.Length > 0 && !int.TryParse(args[0], out count))
                throw new CmdException("Launcher", $"Failed to convert \'{args[0]}\' to int.");
            for (int i = 0; i < count; ++i)
            {
                if (cb.Launcher.MemoryStack.Count == 0)
                    throw new CmdException("Launcher", $"Ran out of items to remove. Cleared {i}/{count}.");
                cb.Launcher.MemoryStack.Pop();
            }
            cb.Launcher.FeedbackLine($"Sucessfully cleared {count} items from memory (now {cb.Launcher.MemoryStack.Count}).");
        }

        private void MemViewCMD(string[] args, Cmd.Callback cb)
        {
            if (cb.Launcher.MemoryStack.Count == 0)
                cb.Launcher.FeedbackLine("No items to view.");
            else
            {
                StringBuilder sb = new();
                foreach (var item in cb.Launcher.MemoryStack)
                    sb.AppendLine($"> \"{item}\"");
                Console.Write(sb.ToString());
            }
        }
       
        private void MemClearCMD(string[] args, Cmd.Callback cb)
        {
            if (cb.Launcher.MemoryStack.Count == 0)
                cb.Launcher.FeedbackLine($"No items to clear.");
            else
            {
                cb.Launcher.FeedbackLine($"Successfully cleared {cb.Launcher.MemoryStack.Count} items from memory.");
                cb.Launcher.MemoryStack.Clear();
            }
        }

        private void MemLockCMD(string[] args, Cmd.Callback cb)
        {
            bool res = !cb.Launcher.MemoryLock;
            if (args.Length > 0 && !bool.TryParse(args[0], out res))
                throw new CmdException("Native", $"Unable to convert \'{args[0]}\'.");
            cb.Launcher.MemoryLock = res;
            cb.Launcher.FeedbackLine($"Memory lock set to {res}.");
        }

        private Cmd.MemItem MemSizeCMD(string[] args, Cmd.Callback cb)
        {
            int size = cb.Launcher.MemoryStack.Count;
            cb.Launcher.FeedbackLine(size);
            return new(size);
        }

        private void MemRunCMD(string[] args, Cmd.Callback cb)
        {
            for (int i = 1; i < args.Length; ++i)
                cb.Launcher.MemoryStack.Push(args[i]);
            cb.Launcher.SExecuteCommand(args[0]);
        }

        private void MemInsCMD(string[] args, Cmd.Callback cb)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                string str = args[i];
                StringBuilder sb = new();
                for (int j = 0; j < str.Length; ++j)
                {
                    if (str[j] != '&')
                        sb.Append(str[j]);
                    else
                    {
                        if (!cb.Launcher.MemoryStack.TryPeek(out var res))
                            throw new CmdException("Launcher", $"Memory is empty.");
                        if (j != str.Length - 1 && str[j + 1] == '^')
                        {
                            cb.Launcher.MemoryStack.Pop();
                            ++j;
                        }
                        sb.Append(res);
                    }
                }
                args[i] = sb.ToString();
            }
            cb.Launcher.ExecuteCommand(args[0], ArrUtils.TrimFirst(args));
        }

        #endregion     

        #endregion
    }
}
